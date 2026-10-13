using System.Text.RegularExpressions;
using MyYt.Domain.Common;

namespace MyYt.Domain.Tags;

/// <summary>A normalized tag name: trimmed, lower-case, single-spaced.</summary>
public sealed partial record TagName
{
    public const int MaxLength = 30;

    private TagName(string value) => Value = value;

    public string Value { get; }

    public static Result<TagName> Create(string? raw)
    {
        var value = Whitespace().Replace((raw ?? string.Empty).Trim().TrimStart('#'), " ").ToLowerInvariant();

        if (value.Length == 0)
            return Error.Validation("tag.empty", "Tag cannot be empty.");
        if (value.Length > MaxLength)
            return Error.Validation("tag.too_long", $"Tag '{value}' is longer than {MaxLength} characters.");
        if (!Allowed().IsMatch(value))
            return Error.Validation("tag.invalid", $"Tag '{value}' may only contain letters, digits, spaces, '-' and '_'.");

        return new TagName(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"^[\p{L}\p{N} _-]+$")]
    private static partial Regex Allowed();
}
