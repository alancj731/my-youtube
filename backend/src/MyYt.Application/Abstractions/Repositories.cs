using MyYt.Application.Common;
using MyYt.Application.Tags;
using MyYt.Application.Videos;
using MyYt.Domain.Tags;
using MyYt.Domain.Users;
using MyYt.Domain.Videos;

namespace MyYt.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct);

    /// <summary>Returns false if a user with the same Firebase UID already exists.</summary>
    Task<bool> AddAsync(User user, CancellationToken ct);

    Task UpdateAsync(User user, CancellationToken ct);
}

public interface IVideoRepository
{
    Task<Video?> GetAsync(Guid userId, Guid videoId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid userId, YouTubeVideoId youTubeId, CancellationToken ct);
    Task<PagedResult<Video>> SearchAsync(Guid userId, VideoSearchCriteria criteria, CancellationToken ct);

    /// <summary>Inserts the video and links its tags, creating missing tags. Returns false if the user already saved it.</summary>
    Task<bool> AddAsync(Video video, CancellationToken ct);

    /// <summary>Replaces the video's tag links and removes tags left unused.</summary>
    Task UpdateTagsAsync(Video video, CancellationToken ct);

    Task<bool> DeleteAsync(Guid userId, Guid videoId, CancellationToken ct);
}

public interface ITagRepository
{
    Task<IReadOnlyList<TagSummary>> ListAsync(Guid userId, CancellationToken ct);
    Task<Tag?> GetAsync(Guid userId, Guid tagId, CancellationToken ct);
    Task<Tag?> GetByNameAsync(Guid userId, TagName name, CancellationToken ct);
    Task RenameAsync(Tag tag, CancellationToken ct);
    Task<bool> DeleteAsync(Guid userId, Guid tagId, CancellationToken ct);
}
