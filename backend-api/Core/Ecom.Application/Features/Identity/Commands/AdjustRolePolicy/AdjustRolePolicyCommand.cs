using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ecom.Application.Features.Identity.Commands.AdjustRolePolicy;

public class AdjustRolePolicyCommand : IRequest<TResult>
{
    [JsonIgnore]
    public Guid RoleId { get; set; }
    public List<Guid> Policies { get; set; } = new List<Guid>();
}

