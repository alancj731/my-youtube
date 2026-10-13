using MyYt.Domain.Tags;
using MyYt.Domain.Videos;

namespace MyYt.Domain.Tests;

public class TagNameTests
{
    [Theory]
    [InlineData("Music", "music")]
    [InlineData("  #Jazz  ", "jazz")]
    [InlineData("lo   fi\tbeats", "lo fi beats")]
    [InlineData("c-sharp_10", "c-sharp_10")]
    [InlineData("音乐", "音乐")]
    public void Normalizes(string raw, string expected)
    {
        var result = TagName.Create(raw);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a,b")]
    [InlineData("<script>")]
    [InlineData("this tag name is way too long to be accepted")]
    public void Rejects_invalid(string raw)
    {
        Assert.True(TagName.Create(raw).IsFailure);
    }

    [Fact]
    public void Video_tags_are_deduplicated_and_sorted()
    {
        var id = YouTubeVideoId.Parse("dQw4w9WgXcQ").Value;
        var tags = new[] { "rock", "Jazz", "rock", "ambient" }.Select(t => TagName.Create(t).Value);

        var video = Video.Create(Guid.NewGuid(), id, "t", "c", DateTimeOffset.UtcNow, tags);

        Assert.Equal(["ambient", "jazz", "rock"], video.Tags.Select(t => t.Value));
    }
}
