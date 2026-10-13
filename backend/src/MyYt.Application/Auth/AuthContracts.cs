namespace MyYt.Application.Auth;

/// <summary>Who the identity provider (Firebase) says is calling, taken from a validated ID token.</summary>
public sealed record ExternalIdentity(string FirebaseUid, string Email, string DisplayName, string? PhotoUrl);

public sealed record UserDto(Guid Id, string Email, string DisplayName, string? PhotoUrl);
