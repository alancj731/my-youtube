using MyYt.Application.Abstractions;
using MyYt.Application.Common;
using MyYt.Domain.Common;
using MyYt.Domain.Videos;

namespace MyYt.Application.Videos;

internal static class VideoErrors
{
    public static readonly Error NotFound = Error.NotFound("video.not_found", "Video not found.");
    public static readonly Error AlreadySaved = Error.Conflict("video.already_saved", "You have already saved this video.");
    public const int MaxTagsPerVideo = 20;

    public static Error TooManyTags => Error.Validation("video.too_many_tags", $"A video can have at most {MaxTagsPerVideo} tags.");
}

public sealed class AddVideoHandler(
    IVideoRepository videos,
    IYouTubeMetadataService metadata,
    ICurrentUser currentUser,
    IClock clock)
{
    public async Task<Result<VideoDto>> HandleAsync(AddVideoRequest request, CancellationToken ct)
    {
        var youTubeId = YouTubeVideoId.Parse(request.Url);
        if (youTubeId.IsFailure)
            return youTubeId.Error!;

        var tags = request.Tags.ToTagNames();
        if (tags.IsFailure)
            return tags.Error!;
        if (tags.Value.Count > VideoErrors.MaxTagsPerVideo)
            return VideoErrors.TooManyTags;

        if (await videos.ExistsAsync(currentUser.Id, youTubeId.Value, ct))
            return VideoErrors.AlreadySaved;

        var meta = await metadata.GetAsync(youTubeId.Value, ct);
        if (meta.IsFailure)
            return meta.Error!;

        var video = Video.Create(currentUser.Id, youTubeId.Value, meta.Value.Title, meta.Value.ChannelName, clock.UtcNow, tags.Value);
        if (!await videos.AddAsync(video, ct))
            return VideoErrors.AlreadySaved;

        return VideoDto.From(video);
    }
}

public sealed class GetVideoHandler(IVideoRepository videos, ICurrentUser currentUser)
{
    public async Task<Result<VideoDto>> HandleAsync(Guid id, CancellationToken ct)
    {
        var video = await videos.GetAsync(currentUser.Id, id, ct);
        return video is null ? VideoErrors.NotFound : VideoDto.From(video);
    }
}

public sealed class SearchVideosHandler(IVideoRepository videos, ICurrentUser currentUser)
{
    public const int MaxPageSize = 100;

    public async Task<Result<PagedResult<VideoDto>>> HandleAsync(SearchVideosRequest request, CancellationToken ct)
    {
        var tags = request.Tags.ToTagNames();
        if (tags.IsFailure)
            return tags.Error!;

        var criteria = new VideoSearchCriteria(
            tags.Value,
            request.MatchAll,
            string.IsNullOrWhiteSpace(request.Query) ? null : request.Query.Trim(),
            Math.Max(1, request.Page),
            Math.Clamp(request.PageSize, 1, MaxPageSize));

        var page = await videos.SearchAsync(currentUser.Id, criteria, ct);
        return page.Map(VideoDto.From);
    }
}

public sealed class UpdateVideoTagsHandler(IVideoRepository videos, ICurrentUser currentUser)
{
    public async Task<Result<VideoDto>> HandleAsync(Guid id, UpdateVideoTagsRequest request, CancellationToken ct)
    {
        var tags = request.Tags.ToTagNames();
        if (tags.IsFailure)
            return tags.Error!;
        if (tags.Value.Count > VideoErrors.MaxTagsPerVideo)
            return VideoErrors.TooManyTags;

        var video = await videos.GetAsync(currentUser.Id, id, ct);
        if (video is null)
            return VideoErrors.NotFound;

        video.SetTags(tags.Value);
        await videos.UpdateTagsAsync(video, ct);
        return VideoDto.From(video);
    }
}

public sealed class DeleteVideoHandler(IVideoRepository videos, ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(Guid id, CancellationToken ct) =>
        await videos.DeleteAsync(currentUser.Id, id, ct) ? Result.Success() : VideoErrors.NotFound;
}
