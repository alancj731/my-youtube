using Microsoft.EntityFrameworkCore;
using MyYt.Application.Abstractions;
using MyYt.Domain.Users;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        (await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct))?.ToDomain();

    public async Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct) =>
        (await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, ct))?.ToDomain();

    public async Task<bool> AddAsync(User user, CancellationToken ct)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            FirebaseUid = user.FirebaseUid,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PhotoUrl = user.PhotoUrl,
            CreatedAt = Mapping.ToUtc(user.CreatedAt),
        };
        db.Users.Add(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            db.Entry(entity).State = EntityState.Detached;
            return false;
        }
    }

    public Task UpdateAsync(User user, CancellationToken ct) =>
        db.Users
            .Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Email, user.Email)
                .SetProperty(u => u.DisplayName, user.DisplayName)
                .SetProperty(u => u.PhotoUrl, user.PhotoUrl), ct);
}
