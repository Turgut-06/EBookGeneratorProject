namespace Domain.Entities;

public class Paper
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalFilePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Book Book { get; set; } = null!;
}