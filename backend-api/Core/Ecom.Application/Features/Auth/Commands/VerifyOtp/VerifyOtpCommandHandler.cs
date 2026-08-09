using Ecom.Application.Common.Configuration;
using Ecom.Domain.Entities;
using Ecom.Domain.Extensions;
using Microsoft.Extensions.Options;

namespace Ecom.Application.Features.Auth.Commands.VerifyOtp;

[EnableUnitOfWork]
public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, TResult<VerifyOtpResult>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IJwtTokenService _jwtTokenService;
	private readonly ILogger<VerifyOtpCommandHandler> _logger;
	private readonly OtpSettings _otpSettings;
	private readonly IOtpSecurityService _otpSecurity;
	private readonly IAuthTokenProtector _tokenProtector;
	private readonly IAuthRateLimitService _rateLimiter;

	public VerifyOtpCommandHandler(
		IUnitOfWork unitOfWork,
		IJwtTokenService jwtTokenService,
		ILogger<VerifyOtpCommandHandler> logger,
		IOptions<OtpSettings> otpOptions,
		IOtpSecurityService otpSecurity,
		IAuthTokenProtector tokenProtector,
		IAuthRateLimitService rateLimiter)
	{
		_unitOfWork = unitOfWork;
		_jwtTokenService = jwtTokenService;
		_logger = logger;
		_otpSettings = otpOptions.Value;
		_otpSecurity = otpSecurity;
		_tokenProtector = tokenProtector;
		_rateLimiter = rateLimiter;
	}

	public async Task<TResult<VerifyOtpResult>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
	{
		try
		{
			if (!VietnamesePhoneNumber.TryNormalize(request.PhoneNumber, out var phoneNumber))
				return TResult<VerifyOtpResult>.Failure(MessageKey.PhoneNumberRequired, ErrorCodes.BAD_REQUEST);

			var user = await GetUserAsync(phoneNumber);
			if (user == null || user.Id == Guid.Empty)
			{
				var missingRateLimit = await _rateLimiter.AcquireAsync(
					AuthRateLimitPolicyNames.OtpVerifyChallenge,
					$"missing:{phoneNumber}",
					cancellationToken);
				if (missingRateLimit.Status == AuthRateLimitStatus.Unavailable)
					return TResult<VerifyOtpResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
				if (missingRateLimit.Status == AuthRateLimitStatus.Rejected)
					return TResult<VerifyOtpResult>.Failure(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS);
				return TResult<VerifyOtpResult>.Failure(MessageKey.AuthenticationFailed, ErrorCodes.UNAUTHORIZED);
			}

			if (IsUserLocked(user))
				return TResult<VerifyOtpResult>.Failure(MessageKey.AuthenticationFailed, ErrorCodes.UNAUTHORIZED);

			// Verify OTP
			var verification = await VerifyOtpAsync(user, request with { PhoneNumber = phoneNumber }, cancellationToken);
			if (verification.RateLimitStatus == AuthRateLimitStatus.Unavailable)
				return TResult<VerifyOtpResult>.Failure(MessageKey.AuthDependencyUnavailable, ErrorCodes.SERVICE_UNAVAILABLE);
			if (verification.RateLimitStatus == AuthRateLimitStatus.Rejected)
				return TResult<VerifyOtpResult>.Failure(MessageKey.TooManyRequests, ErrorCodes.TOO_MANY_REQUESTS);
			if (!verification.IsVerified)
				return TResult<VerifyOtpResult>.Failure(MessageKey.AuthenticationFailed, ErrorCodes.UNAUTHORIZED);

			//  Update user state
			bool isFirstTime = user.LastLoginAt == null;
			UpdateUserAfterLogin(user);

			if (user.Status == UserStatusEnum.Pending)
			{
				user.MarkPhoneVerified();
				user.Activate();
			}

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
			_logger.LogInformation("OTP login succeeded. UserId: {UserId}", user.Id);

			return Success(user, loginStatus, accessToken, refreshToken, policies);
		}
		catch (Exception ex)
		{
			_logger.LogError("OTP verification failed unexpectedly. ExceptionType: {ExceptionType}", ex.GetType().Name);
			return TResult<VerifyOtpResult>.Failure(MessageKey.InternalError, ErrorCodes.SERVER_ERROR);
		}
	}

	// ================= HELPER =================

	private async Task<User?> GetUserAsync(string phone)
	{
		var legacyInternationalPhoneNumber = "84" + phone[1..];
		return await _unitOfWork.Repository<User>()
			.FindOneAsync(
				filters: [u => u.NormalizedPhoneNumber == phone || u.NormalizedPhoneNumber == legacyInternationalPhoneNumber],
				includes: [u => u.Role!]);
	}

	private bool IsUserLocked(User user)
	{
		return user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
	}

	private async Task<OtpVerificationAttempt> VerifyOtpAsync(
		User user,
		VerifyOtpCommand request,
		CancellationToken cancellationToken)
	{
		var testOtp = _otpSecurity.GetTestOtp(request.PhoneNumber, request.ControlledTestBypassKey);
		if (testOtp is not null)
		{
			var testRateLimit = await _rateLimiter.AcquireAsync(
				AuthRateLimitPolicyNames.OtpVerifyChallenge,
				$"{user.Id:N}:test-bypass",
				cancellationToken);
			return new OtpVerificationAttempt(_otpSecurity.Verify(
				user.Id,
				user.Status == UserStatusEnum.Pending ? OtpTokenTypeEnum.ActivateAccount : OtpTokenTypeEnum.Login,
				request.OtpCode,
				_otpSecurity.Protect(
					user.Id,
					user.Status == UserStatusEnum.Pending ? OtpTokenTypeEnum.ActivateAccount : OtpTokenTypeEnum.Login,
				testOtp)), testRateLimit.Status);
		}

		var inferredType = user.Status == UserStatusEnum.Pending
			? OtpTokenTypeEnum.ActivateAccount
			: OtpTokenTypeEnum.Login;

		var otp = await _unitOfWork.Repository<OtpToken>()
			.FindOneAsync(filters: [
				o => o.UserId == user.Id,
				o => o.OtpTokenType == inferredType,
				o => !o.IsUsed
			], orderBy: "CreatedAt desc");

		var challengePartition = otp is null
			? $"{user.Id:N}:no-challenge"
			: $"{otp.Id:N}:{otp.Code}";
		var rateLimit = await _rateLimiter.AcquireAsync(
			AuthRateLimitPolicyNames.OtpVerifyChallenge,
			challengePartition,
			cancellationToken);
		if (rateLimit.Status != AuthRateLimitStatus.Allowed)
			return new OtpVerificationAttempt(false, rateLimit.Status);

		if (otp == null || otp.IsExpired || otp.IsLocked)
			return new OtpVerificationAttempt(false, AuthRateLimitStatus.Allowed);

		if (_otpSecurity.Verify(user.Id, inferredType, request.OtpCode, otp.Code))
		{
			otp.MarkAsUsed();
			await _unitOfWork.Repository<OtpToken>().UpdateAsync(otp);
			return new OtpVerificationAttempt(true, AuthRateLimitStatus.Allowed);
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

		return new OtpVerificationAttempt(false, AuthRateLimitStatus.Allowed);
	}

	private sealed record OtpVerificationAttempt(bool IsVerified, AuthRateLimitStatus RateLimitStatus);

	private void UpdateUserAfterLogin(User user)
	{
		user.FailedLoginAttempts = 0;
		user.LockoutEnd = null;
		user.LastLoginAt = DateTime.UtcNow;
	}

	private string ResolveLoginStatus(User user, bool isFirstTime)
	{
        //  Rule 1: nếu chưa update profile → bắt buộc update (áp dụng cho ALL)
        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            return "OPTIONAL_BASIC_PROFILE";
        }

        // Rule 2: nếu đã update profile → vào app
        return "GO_TO_HOME";
	}

	private async Task SaveRefreshTokenAsync(Guid userId, string token, CancellationToken ct)
	{
		var entity = new JwtRefreshToken
		{
			UserId = userId,
			Token = _tokenProtector.Protect(token),
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
			CanSkipProfile = string.IsNullOrWhiteSpace(user.FullName),
			ProfileState = string.IsNullOrWhiteSpace(user.FullName) ? "BASIC_PROFILE_MISSING" : "READY",
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
