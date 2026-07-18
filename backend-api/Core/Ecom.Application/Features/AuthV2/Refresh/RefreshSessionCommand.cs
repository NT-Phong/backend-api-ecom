namespace Ecom.Application.Features.AuthV2.Refresh;
public sealed record RefreshSessionCommand(string RefreshToken):IRequest<TResult<SessionRefreshResult>>;
public sealed class RefreshSessionValidator:AbstractValidator<RefreshSessionCommand>{public RefreshSessionValidator(){RuleFor(x=>x.RefreshToken).NotEmpty().MaximumLength(512);}}
public sealed class RefreshSessionHandler(ISessionRefreshService service):IRequestHandler<RefreshSessionCommand,TResult<SessionRefreshResult>>
{public Task<TResult<SessionRefreshResult>> Handle(RefreshSessionCommand r,CancellationToken ct)=>service.RotateAsync(r.RefreshToken,ct);}
