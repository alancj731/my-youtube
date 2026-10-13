namespace MyYt.Infrastructure.Persistence.Entities;

public sealed class VideoTagEntity
{
    public Guid VideoId { get; set; }
    public Guid TagId { get; set; }

    public VideoEntity Video { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}
