using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Features.Auth.Commands.VerifyOtp;

[EnableUnitOfWork]
public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, TResult<VerifyOtpResult>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IJwtTokenService _jwtTokenService;
	private readonly ILogger<VerifyOtpCommandHandler> _logger;
	private readonly OtpSettings _otpSettings;

	public VerifyOtpCommandHandler(
		IUnitOfWork unitOfWork,
		IJwtTokenService jwtTokenService,
		ILogger<VerifyOtpCommandHandler> logger,
		IOptions<OtpSettings> otpOptions)
	{
		_unitOfWork = unitOfWork;
		_jwtTokenService = jwtTokenService;
		_logger = logger;
		_otpSettings = otpOptions.Value;
	}

	public async Task<TResult<VerifyOtpResult>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Verifying OTP for PhoneNumber: {PhoneNumber}", request.PhoneNumber);

		try
		{
			var user = await GetUserAsync(request.PhoneNumber);
			if (user == null || user.Id == Guid.Empty)
				return TResult<VerifyOtpResult>.Failure(MessageKey.UserNotFound, ErrorCodes.NOT_FOUND);

			if (IsUserLocked(user))
				return TResult<VerifyOtpResult>.Failure(GetLockMessage(user), ErrorCodes.FORBIDDEN);

			// Verify OTP
			var isVerified = await VerifyOtpAsync(user, request);
			if (!isVerified)
				return TResult<VerifyOtpResult>.Failure(MessageKey.VerificationFailed, ErrorCodes.UNAUTHORIZED);

			//  Update user state
			bool isFirstTime = user.LastLoginAt == null;
			UpdateUserAfterLogin(user);

			if (user.Status == UserStatusEnum.Pending)
				user.Activate();

			user.MarkFirstLogin();
			await _unitOfWork.Repository<User>().UpdateAsync(user);

			//  Policies
			var policies = await GetUserPoliciesAsync(user);

            // Block policies nếu chưa update profile (áp dụng cho User + UnassignedUser)
            //if (user.Role?.Code == global::Ecom.Domain.Constants.Permissions.Roles.User && !user.IsProfileCompleted)
            //{
            //    policies = policies.Where(p => p.StartsWith("user.")).ToList();
            //}

            //  Token
            var accessToken = _jwtTokenService.GenerateAccessToken(user, policies);
			var refreshToken = _jwtTokenService.GenerateRefreshToken();

			await SaveRefreshTokenAsync(user.Id, refreshToken, cancellationToken);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var loginStatus = ResolveLoginStatus(user, isFirstTime);

			return Success(user, loginStatus, accessToken, refreshToken, policies);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error verifying OTP for PhoneNumber: {PhoneNumber}", request.PhoneNumber);
			return TResult<VerifyOtpResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
		}
	}

	// ================= HELPER =================

	private async Task<User?> GetUserAsync(string phone)
	{
		return await _unitOfWork.Repository<User>()
			.FindOneAsync(
				filters: [u => u.PhoneNumber == phone],
				includes: [u => u.Role!]);
	}

	private bool IsUserLocked(User user)
	{
		return user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
	}

	private string GetLockMessage(User user)
	{
		var minutes = (int)(user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes;
		return string.Format(MessageKey.AccountLockedWithMinutes, minutes);
	}

	private async Task<bool> VerifyOtpAsync(User user, VerifyOtpCommand request)
	{
		//  Test account dùng OTP default
		if (IsTestAccount(request.PhoneNumber))
		{
			return request.OtpCode == _otpSettings.DefaultOtp;
		}

		var inferredType = user.Status == UserStatusEnum.Pending
			? OtpTokenTypeEnum.ActivateAccount
			: OtpTokenTypeEnum.Login;

		var otp = await _unitOfWork.Repository<OtpToken>()
			.FindOneAsync(filters: [
				o => o.UserId == user.Id,
				o => o.OtpTokenType == inferredType,
				o => !o.IsUsed
			]);

		if (otp == null || otp.IsExpired || otp.IsLocked)
			return false;

		if (otp.Code == request.OtpCode)
		{
			otp.MarkAsUsed();
			await _unitOfWork.Repository<OtpToken>().UpdateAsync(otp);
			return true;
		}

		//  Sai OTP
		otp.FailedAttempts++;
		otp.UpdatedAt = DateTime.UtcNow;
		await _unitOfWork.Repository<OtpToken>().UpdateAsync(otp);

		user.FailedLoginAttempts++;
		if (user.FailedLoginAttempts >= 5 && user.LockoutEnabled)
		{
			user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
		}

		await _unitOfWork.Repository<User>().UpdateAsync(user);
		await _unitOfWork.SaveChangesAsync();

		return false;
	}

	private bool IsTestAccount(string phone)
	{
		return phone == TestAccounts.Manager
			|| phone == TestAccounts.UnassignedUser;
	}

	private void UpdateUserAfterLogin(User user)
	{
		user.FailedLoginAttempts = 0;
		user.LockoutEnd = null;
		user.LastLoginAt = DateTime.UtcNow;
	}

	private string ResolveLoginStatus(User user, bool isFirstTime)
	{
        //  Rule 1: nếu chưa update profile → bắt buộc update (áp dụng cho ALL)
        if (user.Role?.Code == global::Ecom.Domain.Constants.Permissions.Roles.User && !user.IsProfileCompleted)
        {
            return "REQUIRE_UPDATE_PROFILE";
        }

        // Rule 2: nếu đã update profile → vào app
        return "GO_TO_HOME";
	}

	private async Task SaveRefreshTokenAsync(Guid userId, string token, CancellationToken ct)
	{
		var entity = new JwtRefreshToken
		{
			UserId = userId,
			Token = token,
			ExpiresAt = _jwtTokenService.GetRefreshTokenExpiration(),
			CreatedAt = DateTime.UtcNow
		};

		await _unitOfWork.Repository<JwtRefreshToken>().InsertAsync(entity, ct);
	}

	private TResult<VerifyOtpResult> Success(
		User user,
		string loginStatus,
		string accessToken,
		string refreshToken,
		IEnumerable<string> policies)
	{
		return TResult<VerifyOtpResult>.Success(new VerifyOtpResult
		{
			UserId = user.Id,
			PhoneNumber = user.PhoneNumber,
			IsProfileCompleted = user.IsProfileCompleted,
			LoginStatus = loginStatus,
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			AccessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
			RefreshTokenExpiresAt = _jwtTokenService.GetRefreshTokenExpiration(),
			RoleCode = user.Role?.Code,
			RoleId = user.RoleId,
			RoleName = user.Role?.Name,
			Policies = policies.ToList()
		});
	}

	// ================= POLICIES =================

	private async Task<IEnumerable<string>> GetUserPoliciesAsync(User user)
	{
		var policies = new HashSet<string>();

		if (user.RoleId.HasValue && user.RoleId != Guid.Empty)
		{
			var rolePolicies = await _unitOfWork.Repository<RolePolicy>()
				.FindAsync(
					filters: [rp => rp.RoleId == user.RoleId.Value],
					includes: [rp => rp.Policy!]);

			foreach (var rp in rolePolicies.Where(x => !x.IsDeleted && x.Policy != null && x.Policy.IsActive))
			{
				policies.Add(rp.Policy!.Code);
			}
		}

		var userPolicies = await _unitOfWork.Repository<UserPolicy>()
			.FindAsync(
				filters: [
					up => up.UserId == user.Id,
					up => up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow
				],
				includes: [up => up.Policy!]);

		foreach (var up in userPolicies.Where(x => x.Policy != null && x.Policy.IsActive))
		{
			if (up.IsGranted)
				policies.Add(up.Policy!.Code);
			else
				policies.Remove(up.Policy!.Code);
		}

		return policies;
	}
}
