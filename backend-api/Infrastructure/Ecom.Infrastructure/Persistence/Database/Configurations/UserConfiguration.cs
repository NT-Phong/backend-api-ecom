using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

/// <summary>
/// Configuration for User entity
/// Đăng ký và đăng nhập bằng số điện thoại + OTP
/// </summary>
public class UserConfiguration : BaseEntityConfiguration<User>
{
	public override void Configure(EntityTypeBuilder<User> builder)
	{
		base.Configure(builder);


		// PhoneNumber - unique, required (dùng để đăng ký/đăng nhập)
		builder.Property(u => u.PhoneNumber)
			.HasMaxLength(20)
			.IsRequired();

		builder.HasIndex(u => u.PhoneNumber)
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false");

		// Email - unique, nullable
		builder.Property(u => u.Email)
			.HasMaxLength(255);

		builder.HasIndex(u => u.Email)
			.IsUnique()
			.HasFilter("\"Email\" IS NOT NULL AND \"IsDeleted\" = false");

		// FullName
		builder.Property(u => u.FullName)
			.HasMaxLength(200);

		// AvatarId
		builder.Property(u => u.AvatarId);

		// Address - allow null, max length 500
		builder.Property(u => u.Address)
			.HasMaxLength(500);

		builder.Property(x => x.ZoneIds)
			.HasColumnType("uuid[]");

		// FirstLoginAt - dùng để theo dõi lần đầu đăng nhập
		builder.Property(u => u.FirstLoginAt);

		// IsProfileCompleted - cờ đánh dấu đã hoàn thiện profile chưa
		builder.Property(u => u.IsProfileCompleted)
			.HasDefaultValue(false)
			.IsRequired();

		// Status - enum stored as integer
		builder.Property(u => u.Status)
			.HasConversion<int>()
			.IsRequired();

		// LastLoginIp
		builder.Property(u => u.LastLoginIp)
			.HasMaxLength(45); // IPv6 max length

		// Relationship with Role (1 User has 1 Role)
		builder.HasOne(u => u.Role)
			.WithMany(r => r.Users)
			.HasForeignKey(u => u.RoleId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.HasIndex(u => u.Status);

		// DeletionReason - Lưu lý do khi user thực hiện xóa tài khoản
		builder.Property(u => u.DeletionReason)
			.HasMaxLength(1000);
	}
}
