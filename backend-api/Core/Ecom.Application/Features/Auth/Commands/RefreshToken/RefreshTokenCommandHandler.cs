namespace Ecom.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IJwtTokenService jwtTokenService)
    : IRequestHandler<RefreshTokenCommand, TResult<RefreshTokenResult>>
{
    public async Task<TResult<RefreshTokenResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await jwtTokenService.RefreshJwtToken(request.RefreshToken, cancellationToken);
        return result;
    }
}
