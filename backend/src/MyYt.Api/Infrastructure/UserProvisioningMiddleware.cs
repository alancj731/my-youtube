using Microsoft.Extensions.Caching.Memory;
using MyYt.Application.Auth;

namespace MyYt.Api.Infrastructure;

/// <summary>
/// For requests carrying a valid Firebase ID token, ensures a local user exists for the token's UID
/// and records that user's id for <see cref="HttpCurrentUser"/>.
/// </summary>
internal sealed class UserProvisioningMiddleware(RequestDelegate next, IMemoryCache cache)
{
    public const string UserIdKey = "MyYt.UserId";

    public async Task InvokeAsync(HttpContext context, UserProvisioner provisioner)
    {
        var principal = context.User;
        if (principal.Identity?.IsAuthenticated == true && principal.FindFirst("sub")?.Value is { Length: > 0 } uid)
        {
            var email = principal.FindFirst("email")?.Value ?? string.Empty;
            var identity = new ExternalIdentity(
                uid,
                email,
                principal.FindFirst("name")?.Value ?? string.Empty,
                principal.FindFirst("picture")?.Value);

            // Keyed on the whole identity so a changed name or photo still reaches the database.
            context.Items[UserIdKey] = await cache.GetOrCreateAsync(identity, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return provisioner.EnsureUserAsync(identity, context.RequestAborted);
            });
        }

        await next(context);
    }
}
