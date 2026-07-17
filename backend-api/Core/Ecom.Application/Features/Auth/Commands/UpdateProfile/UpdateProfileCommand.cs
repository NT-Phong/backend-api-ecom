using Ecom.Domain.Models;

namespace Ecom.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<TResult<UpdateProfileResult>>
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public Guid? AvatarId { get; init; }
}

public class UpdateProfileResult : BaseDto
{
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public Guid? AvatarId { get; set; }
}
