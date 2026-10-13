using MyYt.Domain.Tags;
using MyYt.Domain.Users;
using MyYt.Domain.Videos;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence.Repositories;

/// <summary>Converts between EF data models and domain objects. Timestamps are stored as UTC.</summary>
internal static class Mapping
{
    public static DateTime ToUtc(DateTimeOffset value) => value.UtcDateTime;

    public static DateTimeOffset FromUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public static User ToDomain(this UserEntity e) =>
        new(e.Id, e.FirebaseUid, e.Email, e.DisplayName, e.PhotoUrl, FromUtc(e.CreatedAt));

    public static Tag ToDomain(this TagEntity e) =>
        new(e.Id, e.UserId, TagName.Create(e.Name).Value);

    public static Video ToDomain(this VideoEntity e) => new(
        e.Id,
        e.UserId,
        YouTubeVideoId.Parse(e.YouTubeId).Value,
        e.Title,
        e.ChannelName,
        e.ThumbnailUrl,
        FromUtc(e.CreatedAt),
        e.VideoTags.Select(vt => TagName.Create(vt.Tag.Name).Value));
}
