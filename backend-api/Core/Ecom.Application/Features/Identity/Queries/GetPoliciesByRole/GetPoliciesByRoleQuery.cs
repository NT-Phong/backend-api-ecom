using System.Text.Json.Serialization;

namespace Ecom.Application.Features.Identity.Queries.GetPoliciesByRole;

public class GetPoliciesByRoleQuery : IRequest<TResult<List<PoliciesByRoleDto>>>
{
    [JsonIgnore]   
    public Guid RoleId { get; set; }
}

public class PoliciesByRoleDto
{
    public string ModuleGroup { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public List<PolicyCode> Policies { get; set; } = new List<PolicyCode>();
}

public class PolicyCode
{
    public Guid CodeId { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string CodeName { get; set; } = string.Empty;
    public int CodeNo { get; set; }
    public string Type { get; set; }= string.Empty;
    public bool HasPermission { get; set; } = false;
}

