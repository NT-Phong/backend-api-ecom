

namespace Ecom.Domain.Entities;

public class Permission : BaseEntity
{
    public Guid? RoleId { get; set; }
    public Guid? ApiRouteId { get; set; }

    // public ApplicationRole? Role { get; set; }
    // public ApiRoute? ApiRoute { get; set; }
}
