using MyYt.Application.Abstractions;
using MyYt.Domain.Common;
using MyYt.Domain.Tags;

namespace MyYt.Application.Tags;

public sealed class ListTagsHandler(ITagRepository tags, ICurrentUser currentUser)
{
    public Task<IReadOnlyList<TagSummary>> HandleAsync(CancellationToken ct) => tags.ListAsync(currentUser.Id, ct);
}

public sealed class RenameTagHandler(ITagRepository tags, ICurrentUser currentUser)
{
    public async Task<Result<TagSummary>> HandleAsync(Guid id, RenameTagRequest request, CancellationToken ct)
    {
        var name = TagName.Create(request.Name);
        if (name.IsFailure)
            return name.Error!;

        var tag = await tags.GetAsync(currentUser.Id, id, ct);
        if (tag is null)
            return Error.NotFound("tag.not_found", "Tag not found.");

        if (tag.Name != name.Value)
        {
            var existing = await tags.GetByNameAsync(currentUser.Id, name.Value, ct);
            if (existing is not null)
                return Error.Conflict("tag.name_taken", $"You already have a tag named '{name.Value}'.");

            tag.Rename(name.Value);
            await tags.RenameAsync(tag, ct);
        }

        var summary = (await tags.ListAsync(currentUser.Id, ct)).FirstOrDefault(t => t.Id == id);
        return summary ?? new TagSummary(tag.Id, tag.Name.Value, 0);
    }
}

public sealed class DeleteTagHandler(ITagRepository tags, ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(Guid id, CancellationToken ct) =>
        await tags.DeleteAsync(currentUser.Id, id, ct)
            ? Result.Success()
            : Error.NotFound("tag.not_found", "Tag not found.");
}
