using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyYt.Domain.Tags;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.ToTable("tags");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id");

        // Names are normalized by TagName; a binary collation stops MySQL's default
        // accent/case-insensitive collation from treating e.g. "café" and "cafe" as duplicates.
        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(TagName.MaxLength)
            .UseCollation("utf8mb4_bin")
            .IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_tags_users");

        builder.HasIndex(t => new { t.UserId, t.Name }).IsUnique().HasDatabaseName("ux_tags_user_name");
    }
}
