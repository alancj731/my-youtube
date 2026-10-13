using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyYt.Infrastructure.Persistence;

/// <summary>Applies pending EF Core migrations on startup unless <c>Database:MigrateOnStartup</c> is false.</summary>
internal sealed class DatabaseMigrator(
    IServiceScopeFactory scopes,
    IConfiguration configuration,
    ILogger<DatabaseMigrator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        if (!configuration.GetValue("Database:MigrateOnStartup", true))
            return;

        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Tables rely on the server default charset (utf8mb4 on MySQL 8.0+).
        if (await db.Database.CanConnectAsync(ct))
        {
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync(ct);
            logger.LogInformation("Connected to MySQL {Version} at {Server}", connection.ServerVersion, connection.DataSource);
            await connection.CloseAsync();
        }

        var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Database schema is up to date");
            return;
        }

        logger.LogInformation("Applying migrations: {Migrations}", string.Join(", ", pending));
        await db.Database.MigrateAsync(ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
