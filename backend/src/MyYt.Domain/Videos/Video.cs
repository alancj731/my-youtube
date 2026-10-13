using MyYt.Domain.Tags;

namespace MyYt.Domain.Videos;

public sealed class Video
{
    private readonly List<TagName> _tags = [];

    public Video(
        Guid id,
        Guid userId,
        YouTubeVideoId youTubeId,
        string title,
        string? channelName,
        string thumbnailUrl,
        DateTimeOffset createdAt,
        IEnumerable<TagName>? tags = null)
    {
        Id = id;
        UserId = userId;
        YouTubeId = youTubeId;
        Title = title;
        ChannelName = channelName;
        ThumbnailUrl = thumbnailUrl;
        CreatedAt = createdAt;
        if (tags is not null)
            SetTags(tags);
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public YouTubeVideoId YouTubeId { get; }
    public string Title { get; }
    public string? ChannelName { get; }
    public string ThumbnailUrl { get; }
    public DateTimeOffset CreatedAt { get; }
    public IReadOnlyList<TagName> Tags => _tags;

    public static Video Create(
        Guid userId,
        YouTubeVideoId youTubeId,
        string title,
        string? channelName,
        DateTimeOffset now,
        IEnumerable<TagName> tags) =>
        new(Guid.CreateVersion7(now), userId, youTubeId, title, channelName, youTubeId.ThumbnailUrl, now, tags);

    public void SetTags(IEnumerable<TagName> tags)
    {
        // Materialize first: callers may pass a query over the current Tags.
        var normalized = tags.DistinctBy(t => t.Value).OrderBy(t => t.Value, StringComparer.Ordinal).ToList();
        _tags.Clear();
        _tags.AddRange(normalized);
    }
}
