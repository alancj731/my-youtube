using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence.Configurations;

internal sealed class VideoTagConfiguration : IEntityTypeConfiguration<VideoTagEntity>
{
    public void Configure(EntityTypeBuilder<VideoTagEntity> builder)
    {
        builder.ToTable("video_tags");
        builder.HasKey(vt => new { vt.VideoId, vt.TagId });

        builder.Property(vt => vt.VideoId).HasColumnName("video_id");
        builder.Property(vt => vt.TagId).HasColumnName("tag_id");

        builder.HasOne(vt => vt.Video)
            .WithMany(v => v.VideoTags)
            .HasForeignKey(vt => vt.VideoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_video_tags_videos");

        builder.HasOne(vt => vt.Tag)
            .WithMany(t => t.VideoTags)
            .HasForeignKey(vt => vt.TagId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_video_tags_tags");

        builder.HasIndex(vt => vt.TagId).HasDatabaseName("ix_video_tags_tag");
    }
}
