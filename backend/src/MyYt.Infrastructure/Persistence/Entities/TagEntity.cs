namespace MyYt.Infrastructure.Persistence.Entities;

public sealed class TagEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<VideoTagEntity> VideoTags { get; set; } = [];
}
