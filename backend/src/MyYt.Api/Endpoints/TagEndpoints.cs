using MyYt.Api.Infrastructure;
using MyYt.Application.Tags;

namespace MyYt.Api.Endpoints;

internal static class TagEndpoints
{
    public static void MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags").WithTags("Tags").RequireAuthorization();

        group.MapGet("/", async (ListTagsHandler handler, CancellationToken ct) =>
            TypedResults.Ok(await handler.HandleAsync(ct)));

        group.MapPatch("/{id:guid}", async (Guid id, RenameTagRequest request, RenameTagHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(id, request, ct)).ToHttp());

        group.MapDelete("/{id:guid}", async (Guid id, DeleteTagHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(id, ct)).ToHttp());
    }
}
