using Microsoft.EntityFrameworkCore;
using MyYt.Application.Abstractions;
using MyYt.Application.Tags;
using MyYt.Domain.Tags;

namespace MyYt.Infrastructure.Persistence.Repositories;

internal sealed class TagRepository(AppDbContext db) : ITagRepository
{
    public async Task<IReadOnlyList<TagSummary>> ListAsync(Guid userId, CancellationToken ct) =>
        await db.Tags.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.VideoTags.Count).ThenBy(t => t.Name)
            .Select(t => new TagSummary(t.Id, t.Name, t.VideoTags.Count))
            .ToListAsync(ct);

    public async Task<Tag?> GetAsync(Guid userId, Guid tagId, CancellationToken ct) =>
        (await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == userId && t.Id == tagId, ct))?.ToDomain();

    public async Task<Tag?> GetByNameAsync(Guid userId, TagName name, CancellationToken ct) =>
        (await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == userId && t.Name == name.Value, ct))?.ToDomain();

    public Task RenameAsync(Tag tag, CancellationToken ct) =>
        db.Tags
            .Where(t => t.Id == tag.Id && t.UserId == tag.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.Name, tag.Name.Value), ct);

    public async Task<bool> DeleteAsync(Guid userId, Guid tagId, CancellationToken ct) =>
        // video_tags rows go with it via ON DELETE CASCADE.
        await db.Tags.Where(t => t.Id == tagId && t.UserId == userId).ExecuteDeleteAsync(ct) > 0;
}
