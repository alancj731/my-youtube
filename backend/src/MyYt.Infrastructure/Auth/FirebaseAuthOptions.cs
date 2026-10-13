using Microsoft.Extensions.Configuration;

namespace MyYt.Infrastructure.Auth;

/// <summary>Settings for validating Firebase Authentication ID tokens.</summary>
public sealed class FirebaseAuthOptions
{
    public required string ProjectId { get; init; }

    /// <summary>Firebase ID tokens are issued by securetoken.google.com for the project.</summary>
    public string Issuer => $"https://securetoken.google.com/{ProjectId}";

    /// <summary>Reads <c>Firebase:ProjectId</c>, falling back to <c>FIREBASE_PROJECT_ID</c> (e.g. from backend/.env).</summary>
    public static FirebaseAuthOptions FromConfiguration(IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"] is { Length: > 0 } configured
            ? configured
            : configuration["FIREBASE_PROJECT_ID"];

        if (string.IsNullOrWhiteSpace(projectId))
            throw new InvalidOperationException(
                "Firebase is not configured: set FIREBASE_PROJECT_ID in backend/.env (or Firebase:ProjectId).");

        // Project IDs are always lower-case, and token audiences are compared exactly.
        return new FirebaseAuthOptions { ProjectId = projectId.Trim().ToLowerInvariant() };
    }
}
