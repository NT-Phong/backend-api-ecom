using System.Security.Cryptography;
using Ecom.Application.Common.Commerce;
using Microsoft.AspNetCore.Http;

namespace Ecom.Infrastructure.Services;

public sealed class CartPrincipalResolver(IHttpContextAccessor httpContextAccessor, ICurrentUser currentUser) : ICartPrincipalResolver
{
    private const string CookieName = "__Host-ecom_cart";

    public CartPrincipal ResolveOrCreateGuestPrincipal()
    {
        if (currentUser.IsAuthenticated && currentUser.UserId != Guid.Empty)
            return new CartPrincipal(currentUser.UserId, null);

        var context = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HTTP context is available.");
        var token = context.Request.Cookies[CookieName];
        if (string.IsNullOrWhiteSpace(token))
        {
            token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            context.Response.Cookies.Append(CookieName, token, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }
        return new CartPrincipal(null, Hash(token));
    }

    public CartPrincipal? ResolveExistingPrincipal()
    {
        if (currentUser.IsAuthenticated && currentUser.UserId != Guid.Empty)
            return new CartPrincipal(currentUser.UserId, null);
        var token = httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
        return string.IsNullOrWhiteSpace(token) ? null : new CartPrincipal(null, Hash(token));
    }

    public CartPrincipal? ResolveGuestPrincipal()
    {
        var token = httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
        return string.IsNullOrWhiteSpace(token) ? null : new CartPrincipal(null, Hash(token));
    }

    public void ClearGuestPrincipal()
    {
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            Secure = true, SameSite = SameSiteMode.Lax, Path = "/"
        });
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
