namespace MyYt.Api.Infrastructure;

/// <summary>
/// Loads <c>KEY=VALUE</c> pairs from the nearest <c>.env</c> file (searching up from the working directory)
/// into environment variables. Variables that are already set win, so real environment config overrides the file.
/// </summary>
internal static class DotEnv
{
    public static string? Load(string startDirectory)
    {
        var path = Find(startDirectory);
        if (path is null)
            return null;

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            if (line.StartsWith("export ", StringComparison.Ordinal))
                line = line["export ".Length..].TrimStart();

            var separator = line.IndexOf('=');
            if (separator <= 0)
                continue;

            var key = line[..separator].Trim();
            var value = Unquote(line[(separator + 1)..].Trim());
            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
        return path;
    }

    private static string? Find(string directory)
    {
        for (var dir = new DirectoryInfo(directory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate))
                return candidate;
        }
        return null;
    }

    private static string Unquote(string value) =>
        value.Length >= 2 && (value[0] is '"' or '\'') && value[^1] == value[0] ? value[1..^1] : value;
}
