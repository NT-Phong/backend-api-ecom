using System.Text.Json.Serialization;

public record UpdateUserRoleCommand : IRequest<TResult<UpdateUserRoleResult>>
{
    [JsonIgnore]
    public Guid TargetUserId { get; init; }

    public Guid NewRoleId { get; init; } 
}

public class UpdateUserRoleResult
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public Guid NewRoleId { get; init; }
    public string? NewRoleName { get; set; }
    public string? NewRoleCode { get; set; }
    public DateTime UpdatedAt { get; set; }
}