using Ecom.Application.Features.Auth.Queries.GetCurrentUser;

namespace Ecom.Application.Features.Auth.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<TResult<PaginatedList<CurrentUserResult>>>
{
    public Guid? UserId { get; set; }
    public string? SearchText { get; set; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public int Skip() => (Page - 1) * PageSize;
}
