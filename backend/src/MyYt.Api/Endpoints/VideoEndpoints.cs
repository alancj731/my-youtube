using MyYt.Api.Infrastructure;
using MyYt.Application.Videos;

namespace MyYt.Api.Endpoints;

internal static class VideoEndpoints
{
    public static void MapVideoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/videos").WithTags("Videos").RequireAuthorization();

        group.MapGet("/", async (
                string? tags,
                string? match,
                string? q,
                int? page,
                int? pageSize,
                SearchVideosHandler handler,
                CancellationToken ct) =>
            {
                var request = new SearchVideosRequest(
                    tags?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    MatchAll: !string.Equals(match, "any", StringComparison.OrdinalIgnoreCase),
                    Query: q,
                    Page: page ?? 1,
                    PageSize: pageSize ?? 24);
                return (await handler.HandleAsync(request, ct)).ToHttp();
            })
            .WithSummary("Search your videos. tags=a,b  match=all|any  q=title text");

        group.MapGet("/{id:guid}", async (Guid id, GetVideoHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(id, ct)).ToHttp());

        group.MapPost("/", async (AddVideoRequest request, AddVideoHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(request, ct)).ToHttp(video => TypedResults.Created($"/api/videos/{video.Id}", video)));

        group.MapPut("/{id:guid}/tags", async (Guid id, UpdateVideoTagsRequest request, UpdateVideoTagsHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(id, request, ct)).ToHttp());

        group.MapDelete("/{id:guid}", async (Guid id, DeleteVideoHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(id, ct)).ToHttp());
    }
}
