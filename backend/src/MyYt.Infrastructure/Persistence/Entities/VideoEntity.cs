namespace MyYt.Infrastructure.Persistence.Entities;

public sealed class VideoEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string YouTubeId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ChannelName { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<VideoTagEntity> VideoTags { get; set; } = [];
}
