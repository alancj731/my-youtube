using MyYt.Domain.Tags;
using MyYt.Domain.Videos;

namespace MyYt.Application.Videos;

public sealed record AddVideoRequest(string Url, IReadOnlyList<string>? Tags);

public sealed record UpdateVideoTagsRequest(IReadOnlyList<string>? Tags);

public sealed record SearchVideosRequest(
    IReadOnlyList<string>? Tags,
    bool MatchAll = true,
    string? Query = null,
    int Page = 1,
    int PageSize = 24);

/// <summary>Normalized search input handed to the repository.</summary>
public sealed record VideoSearchCriteria(
    IReadOnlyList<TagName> Tags,
    bool MatchAll,
    string? Text,
    int Page,
    int PageSize);

public sealed record VideoDto(
    Guid Id,
    string YouTubeId,
    string Title,
    string? ChannelName,
    string ThumbnailUrl,
    string WatchUrl,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt)
{
    public static VideoDto From(Video video) => new(
        video.Id,
        video.YouTubeId.Value,
        video.Title,
        video.ChannelName,
        video.ThumbnailUrl,
        video.YouTubeId.WatchUrl,
        video.Tags.Select(t => t.Value).ToList(),
        video.CreatedAt);
}
