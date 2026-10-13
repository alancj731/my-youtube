using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MyYt.Application.Abstractions;
using MyYt.Domain.Common;
using MyYt.Domain.Videos;

namespace MyYt.Infrastructure.YouTube;

/// <summary>Looks up title and channel through YouTube's public oEmbed endpoint (no API key).</summary>
internal sealed class OEmbedMetadataService(HttpClient http, ILogger<OEmbedMetadataService> logger) : IYouTubeMetadataService
{
    public async Task<Result<YouTubeMetadata>> GetAsync(YouTubeVideoId id, CancellationToken ct)
    {
        var url = $"oembed?format=json&url={Uri.EscapeDataString(id.WatchUrl)}";
        try
        {
            using var response = await http.GetAsync(url, ct);
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.BadRequest)
                return Error.Validation("video.unavailable", "This video doesn't exist, is private, or can't be embedded.");

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<OEmbedResponse>(ct);
            if (string.IsNullOrWhiteSpace(body?.Title))
                return Error.Validation("video.unavailable", "YouTube returned no details for this video.");

            return new YouTubeMetadata(body.Title.Trim(), body.AuthorName?.Trim());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "oEmbed lookup failed for {VideoId}", id);
            return Error.Unavailable("youtube.unreachable", "Couldn't reach YouTube. Please try again.");
        }
    }

    private sealed record OEmbedResponse(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("author_name")] string? AuthorName);
}
