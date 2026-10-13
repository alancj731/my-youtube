using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence.Configurations;

internal sealed class VideoConfiguration : IEntityTypeConfiguration<VideoEntity>
{
    public void Configure(EntityTypeBuilder<VideoEntity> builder)
    {
        builder.ToTable("videos");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.UserId).HasColumnName("user_id");
        builder.Property(v => v.YouTubeId).HasColumnName("youtube_id").HasMaxLength(11).IsRequired();
        builder.Property(v => v.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(v => v.ChannelName).HasColumnName("channel_name").HasMaxLength(200);
        builder.Property(v => v.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(500).IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_videos_users");

        builder.HasIndex(v => new { v.UserId, v.YouTubeId }).IsUnique().HasDatabaseName("ux_videos_user_youtube");
        builder.HasIndex(v => new { v.UserId, v.CreatedAt }).HasDatabaseName("ix_videos_user_created");
    }
}
