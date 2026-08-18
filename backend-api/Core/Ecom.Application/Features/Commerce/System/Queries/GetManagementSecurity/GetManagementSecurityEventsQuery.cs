using Ecom.Domain.Entities;
using Ecom.Domain.Enums;

namespace Ecom.Application.Features.Commerce.System.Queries.GetManagementSecurity;

public sealed record GetManagementSecurityEventsQuery : IRequest<TResult<PaginatedList<ManagementSecurityEventDto>>>
{ public Guid? UserId { get; init; } public Guid? SessionId { get; init; } public SecurityRiskLevel? RiskLevel { get; init; } public bool? Success { get; init; } public int Page { get; init; } = 1; public int PageSize { get; init; } = 50; public int Skip() => (Page - 1) * PageSize; }
public sealed class GetManagementSecurityEventsQueryValidator : AbstractValidator<GetManagementSecurityEventsQuery>
{ public GetManagementSecurityEventsQueryValidator() { RuleFor(x => x.Page).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); } }
public sealed class GetManagementSecurityEventsQueryHandler(IUnitOfWork uow, ICurrentUser currentUser) : IRequestHandler<GetManagementSecurityEventsQuery, TResult<PaginatedList<ManagementSecurityEventDto>>>
{
    public async Task<TResult<PaginatedList<ManagementSecurityEventDto>>> Handle(GetManagementSecurityEventsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<PaginatedList<ManagementSecurityEventDto>>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.SecurityEvents.Read)) return TResult<PaginatedList<ManagementSecurityEventDto>>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        var query = uow.Repository<SecurityEvent>().QueryNoTracking();
        if (request.UserId.HasValue) query = query.Where(x => x.UserId == request.UserId);
        if (request.SessionId.HasValue) query = query.Where(x => x.SessionId == request.SessionId);
        if (request.RiskLevel.HasValue) query = query.Where(x => x.RiskLevel == request.RiskLevel);
        if (request.Success.HasValue) query = query.Where(x => x.Success == request.Success);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.OccurredAt).Skip(request.Skip()).Take(request.PageSize)
            .Select(x => new ManagementSecurityEventDto(x.Id, x.UserId, x.SessionId, x.EventType, x.RiskLevel, x.Success, x.OccurredAt)).ToListAsync(ct);
        return TResult<PaginatedList<ManagementSecurityEventDto>>.Success(PaginatedList<ManagementSecurityEventDto>.Create(rows, total, request.Page, request.PageSize));
    }
}
