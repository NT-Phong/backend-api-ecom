using Ecom.Domain.Constants;
using Ecom.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolicyEntity = Ecom.Domain.Entities.Policy;
using RoleEntity = Ecom.Domain.Entities.Role;
using RolePolicyEntity = Ecom.Domain.Entities.RolePolicy;

namespace Ecom.Application.Features.Identity.Commands.AdjustRolePolicy;

public class AdjustRolePolicyCommandHandler : IRequestHandler<AdjustRolePolicyCommand, TResult>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<AdjustRolePolicyCommand> _logger;
	private readonly ICurrentUser _currentUserService; // Cần dùng để lấy thông tin người dùng hiện tại

	public AdjustRolePolicyCommandHandler(
		IUnitOfWork unitOfWork,
		ILogger<AdjustRolePolicyCommand> logger,
		ICurrentUser currentUserService)
	{
		_unitOfWork = unitOfWork;
		_logger = logger;
		_currentUserService = currentUserService;
	}

	public async Task<TResult> Handle(AdjustRolePolicyCommand request, CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("Processing Role Policy Adjustment for Role {RoleId} by User {UserId}", request.RoleId, _currentUserService.UserId);

			// 1. Kiểm tra quyền hạn của người đang thực hiện (Ràng buộc phân cấp)
			var currentUserId = _currentUserService.UserId;
			var currentUser = await _unitOfWork.Repository<User>()
				.Query()
				.Include(u => u.Role)
				.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

			if (currentUser == null || currentUser.Role == null)
			{
				return TResult.Failure("Không tìm thấy thông tin tài khoản đang thực hiện.", ErrorCodes.UNAUTHORIZED);
			}

			// 2. Kiểm tra Role mục tiêu (Role cần sửa quyền)
			var targetRole = await _unitOfWork.Repository<RoleEntity>()
				.FindOneAsync(filters: [r => r.Id == request.RoleId]);

			if (targetRole == null)
			{
				return TResult.Failure(MessageKey.RoleNotFound, ErrorCodes.NOT_FOUND);
			}

			// Thực thi Ràng buộc: "Cấp dưới không chỉnh sửa được cấp trên hoặc ngang cấp"
			// Quy tắc: Số Priority càng nhỏ thì quyền càng cao (Admin = 1, Manager = 10, Employee = 20...)
			if (currentUser.Role.Code != Permissions.Roles.Admin) // Nếu không phải là Admin tối cao
			{
				if (currentUser.Role.Priority >= targetRole.Priority)
				{
					return TResult.Failure("Bạn không có quyền chỉnh sửa cấu hình quyền của vai trò ngang cấp hoặc cao hơn.", ErrorCodes.FORBIDDEN);
				}
			}

			// 3. Kiểm tra danh sách Policies truyền lên
			List<Guid> listPolicies = request.Policies ?? new List<Guid>();

			if (!listPolicies.Any())
			{
				return TResult.Failure(MessageKey.PolicyIsRequired, ErrorCodes.BAD_REQUEST);
			}

			var policiesInDb = await _unitOfWork.Repository<PolicyEntity>()
				.FindAsync(filters: [r => listPolicies.Contains(r.Id)]);

			// Kiểm tra xem có ID policy nào gửi lên mà không tồn tại trong DB không
			var missingIds = listPolicies.Except(policiesInDb.Select(p => p.Id)).ToList();
			if (missingIds.Any())
			{
				var missingIdsString = string.Join(", ", missingIds);
				return TResult.Failure($"Các Policy sau không tồn tại trong hệ thống: {missingIdsString}", ErrorCodes.BAD_REQUEST);
			}

			// 4. Lấy tất cả RolePolicy hiện tại (bao gồm cả các bản ghi đã bị xóa mềm)
			var existingRolePolicies = await _unitOfWork.Repository<RolePolicyEntity>()
				.Query(includeDeleted: true)
				.IgnoreQueryFilters()
				.Where(rp => rp.RoleId == request.RoleId)
				.ToListAsync(cancellationToken);

			// Phân loại: Danh sách cần Bật (Active) và danh sách cần Tắt (Delete)
			var rolePoliciesToActive = existingRolePolicies.Where(rp => listPolicies.Contains(rp.PolicyId)).ToList();
			var rolePoliciesToDelete = existingRolePolicies.Where(rp => !listPolicies.Contains(rp.PolicyId)).ToList();

			// 5. Cập nhật dữ liệu

			// BẬT các quyền được chọn nhưng đang ở trạng thái IsDeleted = true
			foreach (var rp in rolePoliciesToActive)
			{
				if (rp.IsDeleted)
				{
					rp.IsDeleted = false;
					rp.UpdatedAt = DateTime.UtcNow;
					await _unitOfWork.Repository<RolePolicyEntity>().UpdateAsync(rp);
				}
			}

			// TẮT các quyền KHÔNG được chọn nhưng đang ở trạng thái IsDeleted = false
			foreach (var rp in rolePoliciesToDelete)
			{
				if (!rp.IsDeleted)
				{
					rp.IsDeleted = true;
					rp.UpdatedAt = DateTime.UtcNow;
					await _unitOfWork.Repository<RolePolicyEntity>().UpdateAsync(rp);
				}
			}

			// TẠO MỚI các RolePolicy chưa từng tồn tại trước đây
			var existingPolicyIds = existingRolePolicies.Select(rp => rp.PolicyId).ToList();
			var newPolicyIds = listPolicies.Except(existingPolicyIds).ToList();

			foreach (var policyId in newPolicyIds)
			{
				await _unitOfWork.Repository<RolePolicyEntity>().InsertAsync(new RolePolicyEntity
				{
					RoleId = request.RoleId,
					PolicyId = policyId,
					CreatedAt = DateTime.UtcNow,
					IsDeleted = false
				});
			}

			// 6. Lưu thay đổi
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("Successfully adjusted RolePolicies for Role {RoleId}. Activated: {ActiveCount}, Deactivated: {DeleteCount}, Created: {NewCount}",
				request.RoleId, rolePoliciesToActive.Count(x => x.IsDeleted == false), rolePoliciesToDelete.Count(x => x.IsDeleted == true), newPolicyIds.Count);

			return TResult.Success();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while adjusting role policies for Role {RoleId}.", request.RoleId);
			return TResult.Failure("An error occurred while adjusting role policies.", ErrorCodes.SERVER_ERROR);
		}
	}
}
