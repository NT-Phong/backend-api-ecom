using Ecom.Domain.Models;

namespace Ecom.Application.Features.Identity.Queries.GetPolicies;

public class GetPoliciesQuery : BaseQueryDto, IRequest<TResult<List<PoliciesDto>>>
{
    public string? ModuleName { get; set; }
    public string? SearchText { get; set; }
}
public class PoliciesDto 
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
    public string Type { get; set; } = string.Empty;
}
