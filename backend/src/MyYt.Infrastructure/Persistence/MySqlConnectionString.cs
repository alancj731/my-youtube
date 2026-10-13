using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace MyYt.Infrastructure.Persistence;

internal static class MySqlConnectionString
{
    /// <summary>
    /// Uses <c>ConnectionStrings:Default</c> when set; otherwise builds the connection string from
    /// <c>MYSQL_HOST</c>, <c>MYSQL_PORT</c>, <c>MYSQL_USER</c>, <c>MYSQL_PASSWORD</c>, <c>MYSQL_DATABASE</c>
    /// and <c>MYSQL_SSL_MODE</c> (e.g. <c>Required</c>; defaults to the connector's <c>Preferred</c>).
    /// </summary>
    public static string Resolve(IConfiguration configuration)
    {
        var explicitValue = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(explicitValue))
            return explicitValue;

        string? Get(string key) => configuration[key] is { Length: > 0 } value ? value : null;

        var host = Get("MYSQL_HOST");
        var user = Get("MYSQL_USER");
        var password = Get("MYSQL_PASSWORD");
        var missing = new[] { ("MYSQL_HOST", host), ("MYSQL_USER", user), ("MYSQL_PASSWORD", password) }
            .Where(x => x.Item2 is null)
            .Select(x => x.Item1)
            .ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"MySQL is not configured: set {string.Join(", ", missing)} in backend/.env (or ConnectionStrings:Default).");

        var port = Get("MYSQL_PORT") is { } p
            ? uint.TryParse(p, out var parsed) ? parsed : throw new InvalidOperationException($"MYSQL_PORT '{p}' is not a number.")
            : 3306u;

        var builder = new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = port,
            UserID = user,
            Password = password,
            Database = Get("MYSQL_DATABASE") ?? "myyt",
            CharacterSet = "utf8mb4",
        };

        if (Get("MYSQL_SSL_MODE") is { } sslMode)
        {
            builder.SslMode = Enum.TryParse<MySqlSslMode>(sslMode, ignoreCase: true, out var mode)
                ? mode
                : throw new InvalidOperationException(
                    $"MYSQL_SSL_MODE '{sslMode}' is not valid. Use one of: {string.Join(", ", Enum.GetNames<MySqlSslMode>())}.");
        }

        return builder.ConnectionString;
    }
}
