using System.Text.RegularExpressions;
using MyYt.Domain.Common;

namespace MyYt.Domain.Videos;

/// <summary>The 11-character id YouTube assigns to every video.</summary>
public sealed partial record YouTubeVideoId
{
    private YouTubeVideoId(string value) => Value = value;

    public string Value { get; }

    public string WatchUrl => $"https://www.youtube.com/watch?v={Value}";
    public string ThumbnailUrl => $"https://i.ytimg.com/vi/{Value}/hqdefault.jpg";

    public static Result<YouTubeVideoId> Parse(string? input)
    {
        var invalid = Error.Validation("video.invalid_url", "That doesn't look like a YouTube video link.");
        var text = input?.Trim();
        if (string.IsNullOrEmpty(text))
            return invalid;

        if (IdPattern().IsMatch(text))
            return new YouTubeVideoId(text);

        if (!text.Contains("://"))
            text = "https://" + text;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
            return invalid;

        var host = uri.Host.ToLowerInvariant();
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? candidate = null;

        if (host is "youtu.be" or "www.youtu.be")
        {
            candidate = segments.FirstOrDefault();
        }
        else if (host == "youtube.com" || host.EndsWith(".youtube.com") || host == "youtube-nocookie.com" || host.EndsWith(".youtube-nocookie.com"))
        {
            if (segments is ["watch", ..])
                candidate = QueryValue(uri.Query, "v");
            else if (segments is [("shorts" or "embed" or "live" or "v" or "e"), var id, ..])
                candidate = id;
        }

        return candidate is not null && IdPattern().IsMatch(candidate)
            ? new YouTubeVideoId(candidate)
            : invalid;
    }

    public override string ToString() => Value;

    private static string? QueryValue(string query, string key)
    {
        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            if (pair.Length == 2 && pair[0] == key)
                return Uri.UnescapeDataString(pair[1]);
        }
        return null;
    }

    [GeneratedRegex(@"^[A-Za-z0-9_-]{11}$")]
    private static partial Regex IdPattern();
}
