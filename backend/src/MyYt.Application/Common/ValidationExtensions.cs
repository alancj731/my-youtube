using MyYt.Domain.Common;
using MyYt.Domain.Tags;

namespace MyYt.Application.Common;

internal static class ValidationExtensions
{
    public static Result<IReadOnlyList<TagName>> ToTagNames(this IEnumerable<string>? raw)
    {
        var tags = new List<TagName>();
        foreach (var item in raw ?? [])
        {
            var tag = TagName.Create(item);
            if (tag.IsFailure)
                return tag.Error!;
            tags.Add(tag.Value);
        }
        return tags.DistinctBy(t => t.Value).ToList();
    }
}
