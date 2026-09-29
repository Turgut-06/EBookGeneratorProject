using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Features.Books.Dtos;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Hosting;

namespace Application.Features.Books.Commands;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, BookResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IPdfGeneratorService _pdfService;

    public CreateBookCommandHandler(IAppDbContext context, IWebHostEnvironment env, IPdfGeneratorService pdfService)
    {
        _context = context;
        _env = env;
        _pdfService = pdfService;
    }

    public async Task<BookResponseDto> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        if (request.files == null)
        {
            throw new ArgumentException("Dosya yükleyiniz");
        }

        var book = new Book
        {
            Title = request.bookName,
            Status = "Processing"
        };

        _context.Books.Add(book);

        string safeBookName = FolderNameHelper.ToSafeFolderName(request.bookName);
        string folderName = $"{safeBookName}-{book.Id}";

        var docxFolder = Path.Combine(_env.WebRootPath, "uploads", "docx", folderName);
        if (!Directory.Exists(docxFolder))
        {
            Directory.CreateDirectory(docxFolder);
        }

        for (int i = 0; i < request.files.Count; i++)
        {
            var file = request.files[i];
            if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"'{file.FileName}' dosya geçerli formatta değildir.");
            }

            var filePath = Path.Combine(docxFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            book.Papers.Add(new Paper
            {
                BookId = book.Id,
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                OriginalFilePath = filePath,
                DisplayOrder = i + 1
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var pdfUrl = await _pdfService.GenerateEbookPdfAsync(
                book.Id,
                safeBookName,
                book.Papers.OrderBy(p => p.DisplayOrder).ToList()
            );

            book.PdfFilePath = pdfUrl;
            book.Status = "Completed";
        }
        catch (Exception ex)
        {
            book.Status = "Failed";
            book.ErrorMessage = $"Pdf oluşturma sırasında bir hata oluştu: {ex.Message}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BookResponseDto(book.Id, book.Title, book.Status, book.PdfFilePath, book.ErrorMessage);
    }
}