using Ecom.Domain.Entities;
using System.Linq.Expressions;
namespace Ecom.Application.Features.Auth.Commands.DeleteAccount;

[EnableUnitOfWork]
public class DeleteAccountCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : IRequestHandler<DeleteAccountCommand, TResult<string>>
{
    public async Task<TResult<string>> Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        // 1. Kiểm tra user hiện tại    
        var currentUserId = currentUser.UserId;
        if (currentUserId == Guid.Empty)
            return TResult<string>.Failure(MessageKey.LoginRequired, ErrorCodes.UNAUTHORIZED);
        // 2. Lấy thông tin user hiện tại
        var user = await unitOfWork.Repository<User>().FindByIdAsync(currentUserId);
        if (user == null)
            return TResult<string>.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);
        // 3. Kiểm tra số điện thoại có khớp với user hiện tại không
        if (user.PhoneNumber != request.PhoneNumber)
            return TResult<string>.Failure(MessageKey.PhoneNumberMismatch, ErrorCodes.BAD_REQUEST);
        // 4. Kiểm tra lý do xóa tài khoản (Phải chọn ít nhất 1 lý do)
        if (!request.SelectedReasons.Any() && string.IsNullOrWhiteSpace(request.OtherReasonNote))
            return TResult<string>.Failure(MessageKey.DeletionReasonRequired, ErrorCodes.BAD_REQUEST);
        // 5.Kiểm tra OTP
        bool isTestAccount = (request.PhoneNumber == TestAccounts.UnassignedUser || request.PhoneNumber == TestAccounts.Manager)
                             && request.OtpCode == "0000";

        if (!isTestAccount)
        {
            var otp = await unitOfWork.Repository<OtpToken>().FindOneAsync(
                filters: new Expression<Func<OtpToken, bool>>[] {
                    o => o.UserId == user.Id,
                    o => o.Code == request.OtpCode,
                    o => !o.IsUsed
                }
            );

            if (otp == null || otp.IsExpired)
                return TResult<string>.Failure(MessageKey.OtpInvalidOrExpired, ErrorCodes.UNAUTHORIZED);

            otp.MarkAsUsed();
            await unitOfWork.Repository<OtpToken>().UpdateAsync(otp);
        }
        // 6. Lưu lý do xóa tài khoản
        var reasons = request.SelectedReasons ?? new List<string>();
        var finalReason = string.Join("; ", reasons);

        if (!string.IsNullOrWhiteSpace(request.OtherReasonNote))
        {
            finalReason = string.IsNullOrEmpty(finalReason)
                ? $"Lý do khác: {request.OtherReasonNote}"
                : $"{finalReason} | Ghi chú thêm: {request.OtherReasonNote}";
        }

        user.DeletionReason = finalReason;

        await unitOfWork.Repository<User>().DeleteAsync(user);

        await unitOfWork.SaveChangesAsync(ct);

        return TResult<string>.Success(MessageKey.DeleteAccountSuccess);
    }
}
