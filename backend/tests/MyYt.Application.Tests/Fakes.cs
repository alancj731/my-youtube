using MyYt.Application.Abstractions;
using MyYt.Application.Common;
using MyYt.Application.Tags;
using MyYt.Application.Videos;
using MyYt.Domain.Common;
using MyYt.Domain.Tags;
using MyYt.Domain.Users;
using MyYt.Domain.Videos;

namespace MyYt.Application.Tests;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

internal sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}

internal sealed class FakeMetadata : IYouTubeMetadataService
{
    public Error? Fail { get; set; }

    public Task<Result<YouTubeMetadata>> GetAsync(YouTubeVideoId id, CancellationToken ct) =>
        Task.FromResult(Fail is null
            ? Result<YouTubeMetadata>.Success(new YouTubeMetadata("Title " + id, "Channel"))
            : Result<YouTubeMetadata>.Failure(Fail));
}

internal sealed class InMemoryUsers : IUserRepository
{
    public List<User> Items { get; } = [];
    public int Updates { get; private set; }

    /// <summary>Simulates another request inserting the same user just before ours.</summary>
    public User? RaceWinner { get; set; }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByFirebaseUidAsync(string uid, CancellationToken ct) =>
        Task.FromResult(Items.FirstOrDefault(u => u.FirebaseUid == uid));

    public Task<bool> AddAsync(User user, CancellationToken ct)
    {
        if (RaceWinner is not null)
        {
            Items.Add(RaceWinner);
            RaceWinner = null;
        }
        if (Items.Any(u => u.FirebaseUid == user.FirebaseUid))
            return Task.FromResult(false);
        Items.Add(user);
        return Task.FromResult(true);
    }

    public Task UpdateAsync(User user, CancellationToken ct)
    {
        Updates++;
        return Task.CompletedTask;
    }
}

/// <summary>Stores videos and derives tags from them, mirroring the SQL repositories' behavior.</summary>
internal sealed class InMemoryLibrary : IVideoRepository, ITagRepository
{
    private readonly Dictionary<(Guid User, string Name), Guid> _tagIds = [];

    public List<Video> Videos { get; } = [];

    public Task<Video?> GetAsync(Guid userId, Guid videoId, CancellationToken ct) =>
        Task.FromResult(Videos.FirstOrDefault(v => v.UserId == userId && v.Id == videoId));

    public Task<bool> ExistsAsync(Guid userId, YouTubeVideoId youTubeId, CancellationToken ct) =>
        Task.FromResult(Videos.Any(v => v.UserId == userId && v.YouTubeId == youTubeId));

    public Task<PagedResult<Video>> SearchAsync(Guid userId, VideoSearchCriteria c, CancellationToken ct)
    {
        var names = c.Tags.Select(t => t.Value).ToHashSet();
        var matches = Videos
            .Where(v => v.UserId == userId)
            .Where(v => names.Count == 0 || (c.MatchAll
                ? names.All(n => v.Tags.Any(t => t.Value == n))
                : v.Tags.Any(t => names.Contains(t.Value))))
            .Where(v => c.Text is null || v.Title.Contains(c.Text, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v.CreatedAt)
            .ToList();
        var page = matches.Skip((c.Page - 1) * c.PageSize).Take(c.PageSize).ToList();
        return Task.FromResult(new PagedResult<Video>(page, c.Page, c.PageSize, matches.Count));
    }

    public Task<bool> AddAsync(Video video, CancellationToken ct)
    {
        Videos.Add(video);
        Sync(video.UserId);
        return Task.FromResult(true);
    }

    public Task UpdateTagsAsync(Video video, CancellationToken ct)
    {
        Sync(video.UserId);
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid userId, Guid videoId, CancellationToken ct)
    {
        var removed = Videos.RemoveAll(v => v.UserId == userId && v.Id == videoId) > 0;
        Sync(userId);
        return Task.FromResult(removed);
    }

    public Task<IReadOnlyList<TagSummary>> ListAsync(Guid userId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<TagSummary>>(_tagIds
            .Where(kv => kv.Key.User == userId)
            .Select(kv => new TagSummary(kv.Value, kv.Key.Name, Videos.Count(v => v.UserId == userId && v.Tags.Any(t => t.Value == kv.Key.Name))))
            .ToList());

    Task<Tag?> ITagRepository.GetAsync(Guid userId, Guid tagId, CancellationToken ct) =>
        Task.FromResult(_tagIds.Where(kv => kv.Key.User == userId && kv.Value == tagId)
            .Select(kv => new Tag(kv.Value, userId, TagName.Create(kv.Key.Name).Value))
            .FirstOrDefault());

    public Task<Tag?> GetByNameAsync(Guid userId, TagName name, CancellationToken ct) =>
        Task.FromResult(_tagIds.TryGetValue((userId, name.Value), out var id) ? new Tag(id, userId, name) : null);

    public Task RenameAsync(Tag tag, CancellationToken ct)
    {
        var old = _tagIds.First(kv => kv.Value == tag.Id).Key;
        _tagIds.Remove(old);
        _tagIds[(tag.UserId, tag.Name.Value)] = tag.Id;
        foreach (var video in Videos.Where(v => v.UserId == tag.UserId))
            video.SetTags(video.Tags.Select(t => t.Value == old.Name ? tag.Name : t));
        return Task.CompletedTask;
    }

    Task<bool> ITagRepository.DeleteAsync(Guid userId, Guid tagId, CancellationToken ct) =>
        throw new NotSupportedException();

    private void Sync(Guid userId)
    {
        var used = Videos.Where(v => v.UserId == userId).SelectMany(v => v.Tags).Select(t => t.Value).ToHashSet();
        foreach (var name in used)
            _tagIds.TryAdd((userId, name), Guid.NewGuid());
        foreach (var key in _tagIds.Keys.Where(k => k.User == userId && !used.Contains(k.Name)).ToList())
            _tagIds.Remove(key);
    }
}
