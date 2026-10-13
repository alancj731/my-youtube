using MyYt.Application.Videos;
using MyYt.Domain.Common;

namespace MyYt.Application.Tests;

public class VideoHandlerTests
{
    private readonly FakeCurrentUser _user = new();
    private readonly FakeClock _clock = new();
    private readonly FakeMetadata _metadata = new();
    private readonly InMemoryLibrary _library = new();

    private AddVideoHandler Add => new(_library, _metadata, _user, _clock);
    private SearchVideosHandler Search => new(_library, _user);

    private async Task<VideoDto> AddAsync(string id, params string[] tags)
    {
        _clock.UtcNow = _clock.UtcNow.AddMinutes(1);
        var result = await Add.HandleAsync(new AddVideoRequest($"https://youtu.be/{id}", tags), default);
        Assert.True(result.IsSuccess, result.Error?.Message);
        return result.Value;
    }

    [Fact]
    public async Task Add_fills_metadata_and_normalizes_tags()
    {
        var video = await AddAsync("dQw4w9WgXcQ", "Music", " #music ", "80s");

        Assert.Equal("Title dQw4w9WgXcQ", video.Title);
        Assert.Equal("Channel", video.ChannelName);
        Assert.Equal(["80s", "music"], video.Tags);
        Assert.Equal("https://i.ytimg.com/vi/dQw4w9WgXcQ/hqdefault.jpg", video.ThumbnailUrl);
    }

    [Fact]
    public async Task Add_rejects_duplicates_for_same_user_only()
    {
        await AddAsync("dQw4w9WgXcQ");

        var again = await Add.HandleAsync(new AddVideoRequest("https://www.youtube.com/watch?v=dQw4w9WgXcQ", null), default);
        Assert.Equal(ErrorType.Conflict, again.Error!.Type);

        _user.Id = Guid.NewGuid();
        Assert.True((await Add.HandleAsync(new AddVideoRequest("dQw4w9WgXcQ", null), default)).IsSuccess);
    }

    [Fact]
    public async Task Add_returns_metadata_failure()
    {
        _metadata.Fail = Error.Validation("video.unavailable", "private");

        var result = await Add.HandleAsync(new AddVideoRequest("dQw4w9WgXcQ", null), default);

        Assert.Equal("video.unavailable", result.Error!.Code);
        Assert.Empty(_library.Videos);
    }

    [Fact]
    public async Task Add_rejects_bad_url_and_bad_tags()
    {
        Assert.Equal("video.invalid_url", (await Add.HandleAsync(new AddVideoRequest("https://vimeo.com/1", null), default)).Error!.Code);
        Assert.Equal("tag.invalid", (await Add.HandleAsync(new AddVideoRequest("dQw4w9WgXcQ", ["a,b"]), default)).Error!.Code);
    }

    [Fact]
    public async Task Search_matches_all_or_any_tags()
    {
        await AddAsync("aaaaaaaaaaa", "jazz", "piano");
        await AddAsync("bbbbbbbbbbb", "jazz");
        await AddAsync("ccccccccccc", "rock");

        var all = await Search.HandleAsync(new SearchVideosRequest(["Jazz", "piano"]), default);
        var any = await Search.HandleAsync(new SearchVideosRequest(["piano", "rock"], MatchAll: false), default);
        var none = await Search.HandleAsync(new SearchVideosRequest(null), default);

        Assert.Equal(["aaaaaaaaaaa"], all.Value.Items.Select(v => v.YouTubeId));
        Assert.Equal(["ccccccccccc", "aaaaaaaaaaa"], any.Value.Items.Select(v => v.YouTubeId));
        Assert.Equal(3, none.Value.TotalCount);
    }

    [Fact]
    public async Task Search_clamps_paging()
    {
        await AddAsync("aaaaaaaaaaa");
        await AddAsync("bbbbbbbbbbb");

        var page = await Search.HandleAsync(new SearchVideosRequest(null, Page: 0, PageSize: 1), default);

        Assert.Equal(1, page.Value.Page);
        Assert.Single(page.Value.Items);
        Assert.True(page.Value.HasMore);
    }

    [Fact]
    public async Task Update_tags_replaces_them_and_hides_other_users_videos()
    {
        var video = await AddAsync("dQw4w9WgXcQ", "old");
        var handler = new UpdateVideoTagsHandler(_library, _user);

        var updated = await handler.HandleAsync(video.Id, new UpdateVideoTagsRequest(["New", "other"]), default);
        Assert.Equal(["new", "other"], updated.Value.Tags);

        _user.Id = Guid.NewGuid();
        var foreign = await handler.HandleAsync(video.Id, new UpdateVideoTagsRequest(["x"]), default);
        Assert.Equal(ErrorType.NotFound, foreign.Error!.Type);
    }

    [Fact]
    public async Task Delete_returns_not_found_when_missing()
    {
        var video = await AddAsync("dQw4w9WgXcQ");
        var handler = new DeleteVideoHandler(_library, _user);

        Assert.True((await handler.HandleAsync(video.Id, default)).IsSuccess);
        Assert.Equal(ErrorType.NotFound, (await handler.HandleAsync(video.Id, default)).Error!.Type);
    }
}
