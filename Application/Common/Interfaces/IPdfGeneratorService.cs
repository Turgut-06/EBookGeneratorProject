using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPdfGeneratorService
{
    Task<string> GenerateEbookPdfAsync(Guid bookId, string safeBookName, List<Paper> papers);
}