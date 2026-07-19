using Ecom.Domain.Common.Interfaces;
using System.Net;
using Ecom.Domain.Extensions;

namespace Ecom.Domain.Entities;

/// <summary>
/// Entity đại diện cho người dùng trong hệ thống
/// Đăng ký và đăng nhập bằng số điện thoại + OTP
/// </summary>
public class User : BaseEntity
{
	#region Authentication Fields

	/// <summary>
	/// Số điện thoại (unique, required - dùng để đăng ký/đăng nhập qua OTP)
	/// </summary>
	public string? PhoneNumber { get; private set; }
	public string? Username { get; private set; }
	public string? NormalizedUsername { get; private set; }
	public string? NormalizedPhoneNumber { get; private set; }

	/// <summary>
	/// Email (optional, có thể dùng để nhận thông báo)
	/// </summary>
	public string? Email { get; private set; }
	public string? NormalizedEmail { get; private set; }
	public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString("N");
	public DateTime? EmailVerifiedAt { get; private set; }
	public DateTime? PhoneVerifiedAt { get; private set; }

	/// <summary>
	/// Số điện thoại đã được xác thực
	/// </summary>
	public bool PhoneNumberConfirmed { get; set; } = false;

	/// <summary>
	/// Email đã được xác thực
	/// </summary>
	public bool EmailConfirmed { get; set; } = false;

	#endregion

	#region User Profile

	/// <summary>
	/// Họ và tên đầy đủ
	/// </summary>
	public string? FullName { get; set; }

	public Guid? AvatarId { get; set; }
	public string? Address { get; set; }

	/// <summary>
	/// Danh sách Zone IDs mà user có thể quản lý/truy cập
	/// </summary>
	public ICollection<Guid> ZoneIds { get; set; } = new List<Guid>();

	public DateTime? FirstLoginAt { get; set; }

	public bool IsProfileCompleted { get; set; } = false;
	public string? DeletionReason { get; set; }

	#endregion

	#region Status & Security

	/// <summary>
	/// Trạng thái tài khoản: Pending, Active, Deactivated
	/// </summary>
	public UserStatusEnum Status { get; set; } = UserStatusEnum.Pending;

	/// <summary>
	/// Số lần đăng nhập sai liên tiếp (reset khi đăng nhập thành công)
	/// </summary>
	public int FailedLoginAttempts { get; set; } = 0;

	/// <summary>
	/// Thời điểm khóa tài khoản tạm thời (null nếu không bị khóa)
	/// </summary>
	public DateTime? LockoutEnd { get; set; }

	/// <summary>
	/// Cho phép khóa tài khoản khi đăng nhập sai nhiều lần
	/// </summary>
	public bool LockoutEnabled { get; set; } = true;

	/// <summary>
	/// Lần đăng nhập cuối cùng
	/// </summary>
	public DateTime? LastLoginAt { get; set; }

	/// <summary>
	/// IP address lần đăng nhập cuối
	/// </summary>
	public string? LastLoginIp { get; set; }

	#endregion

	#region Role & Authorization

	/// <summary>
	/// Role ID của user (1 user có 1 role)
	/// </summary>
	public Guid? RoleId { get; set; }

	/// <summary>
	/// Navigation property đến Role
	/// </summary>
	public virtual Role? Role { get; set; }

	#endregion

	#region Navigation Properties

	/// <summary>
	/// Danh sách Policy được gán/bỏ riêng cho user (ngoài Role)
	/// </summary>
	public virtual ICollection<UserPolicy> UserPolicies { get; set; } = new List<UserPolicy>();

	/// <summary>
	/// Danh sách OTP tokens của user
	/// </summary>
	public virtual ICollection<OtpToken> OtpTokens { get; set; } = new List<OtpToken>();

	/// <summary>
	/// Danh sách Refresh tokens của user
	/// </summary>
	public virtual ICollection<JwtRefreshToken> RefreshTokens { get; set; } = new List<JwtRefreshToken>();

	/// <summary>
	/// Danh sách FCM Device Tokens của user (đăng ký Push Notification)
	/// </summary>
	public virtual ICollection<UserDeviceToken> DeviceTokens { get; set; } = new List<UserDeviceToken>();

	#endregion

	#region Constructors

	private User() { }

	public User(string? phoneNumber, Guid? roleId)
	{
		SetPhoneNumber(phoneNumber);
		RoleId = roleId;
		Status = UserStatusEnum.Pending;
		IsProfileCompleted = false;
		PhoneNumberConfirmed = false;
	}

	public User(string fullName, string? phoneNumber, Guid? roleId, ICollection<Guid>? zoneIds, UserStatusEnum status)
	{
		this.FullName = fullName;
		SetPhoneNumber(phoneNumber);
		this.RoleId = roleId;
		this.ZoneIds = zoneIds ?? new List<Guid>();
		this.Status = status;
	}

	#endregion

	#region Methods

	public void MarkFirstLogin()
	{
		if (FirstLoginAt == null) FirstLoginAt = DateTime.UtcNow;
	}

	public void Activate()
	{
		if (PhoneVerifiedAt is null && EmailVerifiedAt is null)
			throw new InvalidOperationException("An active user requires a verified contact or external identity.");
		Status = UserStatusEnum.Active;
	}

	public void SetPhoneNumber(string? phoneNumber)
	{
		PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : VietnamesePhoneNumber.Normalize(phoneNumber);
		NormalizedPhoneNumber = PhoneNumber;
	}

	public void SetEmail(string? email)
	{
		Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
		NormalizedEmail = Email?.ToUpperInvariant();
	}

	public void SetUsername(string username)
	{
		if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
		Username = username.Trim();
		NormalizedUsername = Username.ToUpperInvariant();
	}

	public void MarkPhoneVerified(DateTime? occurredAt = null)
	{
		if (PhoneNumber is null) throw new InvalidOperationException("A phone number is required.");
		PhoneVerifiedAt = occurredAt ?? DateTime.UtcNow;
		PhoneNumberConfirmed = true;
	}

	public void MarkEmailVerified(DateTime? occurredAt = null)
	{
		if (Email is null) throw new InvalidOperationException("An email is required.");
		EmailVerifiedAt = occurredAt ?? DateTime.UtcNow;
		EmailConfirmed = true;
	}

	public void RotateSecurityStamp() => SecurityStamp = Guid.NewGuid().ToString("N");

	public void CompleteProfile(string fullName, string? email, string? address, Guid? avatarId)
	{
		SetBasicProfile(fullName);
		SetEmail(email);
		Address = address;
		AvatarId = avatarId;
		IsProfileCompleted = true;
	}

	public void SetBasicProfile(string fullName)
	{
		if (string.IsNullOrWhiteSpace(fullName))
			throw new ArgumentException("Full name is required.", nameof(fullName));

		FullName = fullName.Trim();
	}

	/// <summary>
	/// Thêm một Zone ID vào danh sách
	/// </summary>
	public void AddZoneId(Guid zoneId)
	{
		if (!ZoneIds.Contains(zoneId))
		{
			ZoneIds.Add(zoneId);
		}
	}

	/// <summary>
	/// Xóa một Zone ID từ danh sách
	/// </summary>
	public void RemoveZoneId(Guid zoneId)
	{
		ZoneIds.Remove(zoneId);
	}

	/// <summary>
	/// Cập nhật danh sách Zone IDs
	/// </summary>
	public void UpdateZoneIds(ICollection<Guid>? zoneIds)
	{
		ZoneIds = zoneIds ?? new List<Guid>();
	}

	/// <summary>
	/// Kiểm tra user có quyền truy cập zone hay không
	/// </summary>
	public bool HasAccessToZone(Guid zoneId)
	{
		return ZoneIds.Contains(zoneId);
	}

	#endregion
}
