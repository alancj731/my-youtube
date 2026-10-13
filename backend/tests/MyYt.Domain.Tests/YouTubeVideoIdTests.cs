using MyYt.Domain.Videos;

namespace MyYt.Domain.Tests;

public class YouTubeVideoIdTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?feature=share&v=dQw4w9WgXcQ&t=42")]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://music.youtube.com/watch?v=dQw4w9WgXcQ&list=RD")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?si=abc")]
    [InlineData("youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/live/dQw4w9WgXcQ?feature=share")]
    [InlineData("  dQw4w9WgXcQ  ")]
    public void Parses_known_url_shapes(string input)
    {
        var result = YouTubeVideoId.Parse(input);

        Assert.True(result.IsSuccess);
        Assert.Equal("dQw4w9WgXcQ", result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://vimeo.com/123456")]
    [InlineData("https://www.youtube.com/")]
    [InlineData("https://www.youtube.com/watch?v=short")]
    [InlineData("https://evil-youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/@somechannel")]
    public void Rejects_non_video_input(string? input)
    {
        var result = YouTubeVideoId.Parse(input);

        Assert.True(result.IsFailure);
        Assert.Equal("video.invalid_url", result.Error!.Code);
    }

    [Fact]
    public void Builds_thumbnail_and_watch_urls()
    {
        var id = YouTubeVideoId.Parse("dQw4w9WgXcQ").Value;

        Assert.Equal("https://i.ytimg.com/vi/dQw4w9WgXcQ/hqdefault.jpg", id.ThumbnailUrl);
        Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", id.WatchUrl);
    }
}
