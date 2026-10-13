using MyYt.Api.Infrastructure;
using MyYt.Application.Auth;

namespace MyYt.Api.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Sign-in happens in the browser with Firebase; the API only needs to know who is calling.
        app.MapGroup("/api/auth").WithTags("Auth")
            .MapGet("/me", async (GetCurrentUserHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(ct)).ToHttp())
            .RequireAuthorization();
    }
}
