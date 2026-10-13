namespace MyYt.Domain.Tags;

public sealed class Tag
{
    public Tag(Guid id, Guid userId, TagName name)
    {
        Id = id;
        UserId = userId;
        Name = name;
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public TagName Name { get; private set; }

    public static Tag Create(Guid userId, TagName name) => new(Guid.CreateVersion7(), userId, name);

    public void Rename(TagName name) => Name = name;
}
