using MyYt.Application.Tags;
using MyYt.Application.Videos;
using MyYt.Domain.Common;

namespace MyYt.Application.Tests;

public class TagHandlerTests
{
    private readonly FakeCurrentUser _user = new();
    private readonly InMemoryLibrary _library = new();

    private async Task SeedAsync(string id, params string[] tags) =>
        await new AddVideoHandler(_library, new FakeMetadata(), _user, new FakeClock())
            .HandleAsync(new AddVideoRequest(id, tags), default);

    [Fact]
    public async Task Rename_to_existing_name_conflicts()
    {
        await SeedAsync("aaaaaaaaaaa", "jazz", "rock");
        var tags = await new ListTagsHandler(_library, _user).HandleAsync(default);
        var jazz = tags.Single(t => t.Name == "jazz");

        var result = await new RenameTagHandler(_library, _user).HandleAsync(jazz.Id, new RenameTagRequest("Rock"), default);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public async Task Rename_updates_videos()
    {
        await SeedAsync("aaaaaaaaaaa", "jazz");
        var jazz = (await new ListTagsHandler(_library, _user).HandleAsync(default)).Single();

        var result = await new RenameTagHandler(_library, _user).HandleAsync(jazz.Id, new RenameTagRequest("Smooth Jazz"), default);

        Assert.Equal(new TagSummary(jazz.Id, "smooth jazz", 1), result.Value);
        Assert.Equal(["smooth jazz"], _library.Videos.Single().Tags.Select(t => t.Value));
    }
}
