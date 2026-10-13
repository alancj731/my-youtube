using Microsoft.EntityFrameworkCore;
using MyYt.Application.Abstractions;
using MyYt.Application.Common;
using MyYt.Application.Videos;
using MyYt.Domain.Videos;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence.Repositories;

internal sealed class VideoRepository(AppDbContext db) : IVideoRepository
{
    private IQueryable<VideoEntity> VideosWithTags =>
        db.Videos.AsNoTracking().Include(v => v.VideoTags).ThenInclude(vt => vt.Tag);

    public async Task<Video?> GetAsync(Guid userId, Guid videoId, CancellationToken ct) =>
        (await VideosWithTags.FirstOrDefaultAsync(v => v.UserId == userId && v.Id == videoId, ct))?.ToDomain();

    public Task<bool> ExistsAsync(Guid userId, YouTubeVideoId youTubeId, CancellationToken ct) =>
        db.Videos.AnyAsync(v => v.UserId == userId && v.YouTubeId == youTubeId.Value, ct);

    public async Task<PagedResult<Video>> SearchAsync(Guid userId, VideoSearchCriteria criteria, CancellationToken ct)
    {
        var query = db.Videos.AsNoTracking().Where(v => v.UserId == userId);

        if (criteria.Text is { } text)
        {
            var pattern = "%" + EscapeLike(text) + "%";
            query = query.Where(v =>
                EF.Functions.Like(v.Title, pattern, LikeEscape) ||
                (v.ChannelName != null && EF.Functions.Like(v.ChannelName, pattern, LikeEscape)));
        }

        if (criteria.Tags.Count > 0)
        {
            var names = criteria.Tags.Select(t => t.Value).ToList();
            query = criteria.MatchAll
                ? query.Where(v => v.VideoTags.Count(vt => names.Contains(vt.Tag.Name)) == names.Count)
                : query.Where(v => v.VideoTags.Any(vt => names.Contains(vt.Tag.Name)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(v => v.VideoTags).ThenInclude(vt => vt.Tag)
            .OrderByDescending(v => v.CreatedAt).ThenByDescending(v => v.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .AsSplitQuery()
            .ToListAsync(ct);

        return new PagedResult<Video>(items.Select(v => v.ToDomain()).ToList(), criteria.Page, criteria.PageSize, total);
    }

    public async Task<bool> AddAsync(Video video, CancellationToken ct)
    {
        var entity = new VideoEntity
        {
            Id = video.Id,
            UserId = video.UserId,
            YouTubeId = video.YouTubeId.Value,
            Title = video.Title,
            ChannelName = video.ChannelName,
            ThumbnailUrl = video.ThumbnailUrl,
            CreatedAt = Mapping.ToUtc(video.CreatedAt),
        };
        entity.VideoTags.AddRange((await ResolveTagsAsync(video, ct)).Select(tag => new VideoTagEntity { Tag = tag }));
        db.Videos.Add(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            db.ChangeTracker.Clear();
            // Only report "already saved" if that is the constraint that failed (not, say, a concurrent tag insert).
            if (await ExistsAsync(video.UserId, video.YouTubeId, ct))
                return false;
            throw;
        }
    }

    public async Task UpdateTagsAsync(Video video, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var entity = await db.Videos
            .Include(v => v.VideoTags).ThenInclude(vt => vt.Tag)
            .SingleAsync(v => v.Id == video.Id && v.UserId == video.UserId, ct);

        var wanted = video.Tags.Select(t => t.Value).ToHashSet(StringComparer.Ordinal);
        entity.VideoTags.RemoveAll(vt => !wanted.Contains(vt.Tag.Name));

        var existing = entity.VideoTags.Select(vt => vt.Tag.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var tag in await ResolveTagsAsync(video, ct))
        {
            if (!existing.Contains(tag.Name))
                entity.VideoTags.Add(new VideoTagEntity { Tag = tag });
        }

        await db.SaveChangesAsync(ct);
        await DeleteUnusedTagsAsync(video.UserId, ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid videoId, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        // video_tags rows go with it via ON DELETE CASCADE.
        var deleted = await db.Videos.Where(v => v.Id == videoId && v.UserId == userId).ExecuteDeleteAsync(ct);
        if (deleted > 0)
            await DeleteUnusedTagsAsync(userId, ct);

        await transaction.CommitAsync(ct);
        return deleted > 0;
    }

    /// <summary>Returns tracked tag entities for the video's tags, creating any the user doesn't have yet.</summary>
    private async Task<List<TagEntity>> ResolveTagsAsync(Video video, CancellationToken ct)
    {
        var names = video.Tags.Select(t => t.Value).ToList();
        if (names.Count == 0)
            return [];

        var tags = await db.Tags.Where(t => t.UserId == video.UserId && names.Contains(t.Name)).ToListAsync(ct);
        foreach (var name in names.Except(tags.Select(t => t.Name), StringComparer.Ordinal))
        {
            var tag = new TagEntity { Id = Guid.CreateVersion7(), UserId = video.UserId, Name = name };
            db.Tags.Add(tag);
            tags.Add(tag);
        }
        return tags;
    }

    private Task<int> DeleteUnusedTagsAsync(Guid userId, CancellationToken ct) =>
        db.Tags.Where(t => t.UserId == userId && !t.VideoTags.Any()).ExecuteDeleteAsync(ct);

    // '!' rather than '\\': the MySQL provider emits ESCAPE '\\' unescaped, which breaks the SQL string literal.
    private const string LikeEscape = "!";

    private static string EscapeLike(string value) =>
        value.Replace("!", "!!").Replace("%", "!%").Replace("_", "!_");
}
