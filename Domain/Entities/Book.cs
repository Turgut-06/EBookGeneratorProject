using System.IO.Pipelines;

namespace Domain.Entities;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? PdfFilePath { get; set; }
    public string Status { get; set; } = "Pending"; 
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Paper> Papers { get; set; } = new List<Paper>();
}