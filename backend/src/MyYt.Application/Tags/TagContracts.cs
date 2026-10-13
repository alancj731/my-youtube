namespace MyYt.Application.Tags;

public sealed record TagSummary(Guid Id, string Name, int VideoCount);

public sealed record RenameTagRequest(string Name);
