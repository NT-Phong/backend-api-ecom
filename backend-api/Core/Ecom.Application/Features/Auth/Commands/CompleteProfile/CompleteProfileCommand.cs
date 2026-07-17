using System.Text.Json.Serialization;

namespace Ecom.Application.Features.Auth.Commands.CompleteProfile;

public record CompleteProfileCommand : IRequest<TResult<CompleteProfileResult>>, IUserRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Address { get; init; }
    public Guid? AvatarId { get; init; }
}
public class CompleteProfileResult
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public Guid? AvatarId { get; set; }
    public bool IsProfileCompleted { get; set; }
}
