using MyYt.Application.Auth;
using MyYt.Domain.Users;

namespace MyYt.Application.Tests;

public class UserProvisionerTests
{
    private readonly InMemoryUsers _users = new();
    private readonly FakeClock _clock = new();

    private UserProvisioner Provisioner => new(_users, _clock);

    private static ExternalIdentity Alice(string name = "Alice", string? photo = "https://example.com/a.png") =>
        new("firebase-uid-alice", " Alice@Example.com ", name, photo);

    [Fact]
    public async Task First_sign_in_creates_a_user()
    {
        var id = await Provisioner.EnsureUserAsync(Alice(), default);

        var user = Assert.Single(_users.Items);
        Assert.Equal(id, user.Id);
        Assert.Equal("firebase-uid-alice", user.FirebaseUid);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("Alice", user.DisplayName);
        Assert.Equal("https://example.com/a.png", user.PhotoUrl);
    }

    [Fact]
    public async Task Repeat_sign_in_reuses_the_user_without_writes()
    {
        var first = await Provisioner.EnsureUserAsync(Alice(), default);
        var second = await Provisioner.EnsureUserAsync(Alice(), default);

        Assert.Equal(first, second);
        Assert.Single(_users.Items);
        Assert.Equal(0, _users.Updates);
    }

    [Fact]
    public async Task Changed_profile_is_saved()
    {
        await Provisioner.EnsureUserAsync(Alice(), default);

        await Provisioner.EnsureUserAsync(Alice(name: "Alice B", photo: null), default);

        Assert.Equal(1, _users.Updates);
        Assert.Equal("Alice B", _users.Items[0].DisplayName);
        Assert.Null(_users.Items[0].PhotoUrl);
    }

    [Fact]
    public async Task Missing_name_falls_back_to_email_prefix()
    {
        await Provisioner.EnsureUserAsync(Alice(name: "  "), default);

        Assert.Equal("alice", _users.Items[0].DisplayName);
    }

    [Fact]
    public async Task Concurrent_first_sign_in_returns_the_existing_user()
    {
        var winner = User.Create("firebase-uid-alice", "alice@example.com", "Alice", null, _clock.UtcNow);
        _users.RaceWinner = winner;

        var id = await Provisioner.EnsureUserAsync(Alice(), default);

        Assert.Equal(winner.Id, id);
        Assert.Single(_users.Items);
    }
}
