using MyYt.Application.Abstractions;
using MyYt.Domain.Common;
using MyYt.Domain.Users;

namespace MyYt.Application.Auth;

/// <summary>Maps a Firebase identity to a local user, creating the user on first sign-in.</summary>
public sealed class UserProvisioner(IUserRepository users, IClock clock)
{
    public async Task<Guid> EnsureUserAsync(ExternalIdentity identity, CancellationToken ct)
    {
        var existing = await users.GetByFirebaseUidAsync(identity.FirebaseUid, ct);
        if (existing is not null)
        {
            if (existing.UpdateProfile(identity.Email, identity.DisplayName, identity.PhotoUrl))
                await users.UpdateAsync(existing, ct);
            return existing.Id;
        }

        var user = User.Create(identity.FirebaseUid, identity.Email, identity.DisplayName, identity.PhotoUrl, clock.UtcNow);
        if (await users.AddAsync(user, ct))
            return user.Id;

        // A parallel first request created the user in the meantime.
        var created = await users.GetByFirebaseUidAsync(identity.FirebaseUid, ct)
            ?? throw new InvalidOperationException($"User {identity.FirebaseUid} could not be created or found.");
        return created.Id;
    }
}

public sealed class GetCurrentUserHandler(IUserRepository users, ICurrentUser currentUser)
{
    public async Task<Result<UserDto>> HandleAsync(CancellationToken ct)
    {
        var user = await users.GetByIdAsync(currentUser.Id, ct);
        return user is null
            ? Error.Unauthorized("auth.user_missing", "The account no longer exists.")
            : new UserDto(user.Id, user.Email, user.DisplayName, user.PhotoUrl);
    }
}
