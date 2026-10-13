namespace MyYt.Domain.Users;

/// <summary>A person signed in through Firebase Authentication, identified by their Firebase UID.</summary>
public sealed class User
{
    public User(Guid id, string firebaseUid, string email, string displayName, string? photoUrl, DateTimeOffset createdAt)
    {
        Id = id;
        FirebaseUid = firebaseUid;
        Email = email;
        DisplayName = displayName;
        PhotoUrl = photoUrl;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public string FirebaseUid { get; }
    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public string? PhotoUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public static User Create(string firebaseUid, string email, string displayName, string? photoUrl, DateTimeOffset now)
    {
        var normalizedEmail = NormalizeEmail(email);
        return new(Guid.CreateVersion7(now), firebaseUid, normalizedEmail, NormalizeName(displayName, normalizedEmail), photoUrl, now);
    }

    /// <summary>Copies profile details from the identity provider; returns true if anything changed.</summary>
    public bool UpdateProfile(string email, string displayName, string? photoUrl)
    {
        var normalizedEmail = NormalizeEmail(email);
        var normalizedName = NormalizeName(displayName, normalizedEmail);
        if (normalizedEmail == Email && normalizedName == DisplayName && photoUrl == PhotoUrl)
            return false;

        Email = normalizedEmail;
        DisplayName = normalizedName;
        PhotoUrl = photoUrl;
        return true;
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string NormalizeName(string displayName, string normalizedEmail)
    {
        var name = displayName.Trim();
        if (name.Length == 0)
            name = normalizedEmail.Split('@')[0];
        return name.Length > 100 ? name[..100] : name;
    }
}
