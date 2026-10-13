using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.FirebaseUid).HasColumnName("firebase_uid").HasMaxLength(128).IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.Property(u => u.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.PhotoUrl).HasColumnName("photo_url").HasMaxLength(1000);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();

        // Identity is the Firebase UID; email is informational and may repeat across providers.
        builder.HasIndex(u => u.FirebaseUid).IsUnique().HasDatabaseName("ux_users_firebase_uid");
        builder.HasIndex(u => u.Email).HasDatabaseName("ix_users_email");
    }
}
