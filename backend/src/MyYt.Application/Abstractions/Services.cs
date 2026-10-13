using MyYt.Domain.Common;
using MyYt.Domain.Videos;

namespace MyYt.Application.Abstractions;

public interface ICurrentUser
{
    /// <summary>Id of the authenticated user; throws if the request is anonymous.</summary>
    Guid Id { get; }
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed record YouTubeMetadata(string Title, string? ChannelName);

public interface IYouTubeMetadataService
{
    Task<Result<YouTubeMetadata>> GetAsync(YouTubeVideoId id, CancellationToken ct);
}
