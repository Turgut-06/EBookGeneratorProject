using System.Reflection.Metadata;
using Application.Common.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly IWebHostEnvironment _env;
    private readonly IContactCleanerService _cleaner;

    public PdfGeneratorService(IWebHostEnvironment env, IContactCleanerService cleaner)
    {
        _env = env;
        _cleaner = cleaner;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateEbookPdfAsync(Guid bookId, string safeBookName, List<Paper> papers)
    {
        var pdfsFolder = Path.Combine(_env.WebRootPath, "uploads", "pdfs");
        if (!Directory.Exists(pdfsFolder))
        {
            Directory.CreateDirectory(pdfsFolder);
        }

        var pdfFileName = $"{safeBookName}-{bookId}.pdf";
        var fullPdfPath = Path.Combine(pdfsFolder, pdfFileName);

        var parsedPapers = new List<ParsedPaperModel>();

        foreach (var paper in papers.OrderBy(p => p.DisplayOrder))
        {
            var paragraphs = ReadAndCleanDocx(paper.OriginalFilePath);
            parsedPapers.Add(new ParsedPaperModel
            {
                Order = paper.DisplayOrder,
                Title = paper.Title,
                Paragraphs = paragraphs
            });
        }

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Height(30)
                    .AlignRight()
                    .Text(safeBookName.Replace("-", " ").ToUpper())
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);

                page.Content().Column(column =>
                {
                    //  Kapak Sayfası
                    column.Item().Height(600).Column(cover =>
                    {
                        cover.Item().PaddingTop(150).AlignCenter().Text(safeBookName.Replace("-", " ").ToUpper())
                            .FontSize(28).Bold().FontColor(Colors.Blue.Darken3);

                        cover.Item().PaddingTop(20).AlignCenter().Text("Bildiriler Kitabı (E-Book)")
                            .FontSize(16).Italic();
                    });

                    column.Item().PageBreak();

                    // İçindekiler Tablosu
                    column.Item().PaddingBottom(15).Text("İÇİNDEKİLER")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken3);

                    column.Item().PaddingBottom(20).Column(toc =>
                    {
                        foreach (var paper in parsedPapers)
                        {
                            toc.Item().PaddingVertical(5).Row(row =>
                            {
                                row.RelativeItem().Text($"{paper.Order}. {paper.Title}").Bold();

                                row.AutoItem().Text(x =>
                                {
                                    x.BeginPageNumberOfSection($"paper_{paper.Order}");
                                });
                            });
                        }
                    });

                    column.Item().PageBreak();

                    // Bildiriler
                    foreach (var paper in parsedPapers)
                    {
                        column.Item().Section($"paper_{paper.Order}").Column(paperCol =>
                        {
                            paperCol.Item().PaddingBottom(10).Text($"{paper.Order}. {paper.Title}")
                                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);

                            foreach (var paragraph in paper.Paragraphs)
                            {
                                if (!string.IsNullOrWhiteSpace(paragraph))
                                {
                                    paperCol.Item().PaddingBottom(8).Text(paragraph)
                                        .Justify().LineHeight(1.2f);
                                }
                            }
                        });

                        if (paper != parsedPapers.Last())
                        {
                            column.Item().PageBreak();
                        }
                    }
                });

                // Tutarlı Sayfa Numarası
                page.Footer()
                    .Height(30)
                    .AlignCenter()
                    .Text(x =>
                    {
                         x.Span("Sayfa ");
                         x.CurrentPageNumber();
                         x.Span(" / ");
                         x.TotalPages();
                    });

                page.DefaultTextStyle(x => x
                     .FontFamily("Lato") 
                     .FontSize(11)
                     .FontColor("#333333")
);
            });
        });

        await Task.Run(() => document.GeneratePdf(fullPdfPath));

        return $"/uploads/pdfs/{pdfFileName}";
    }

    private List<string> ReadAndCleanDocx(string filePath)
    {
        var cleanedParagraphs = new List<string>();

        if (!File.Exists(filePath)) return cleanedParagraphs;

        using (var doc = WordprocessingDocument.Open(filePath, false))
        {
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    string rawText = paragraph.InnerText;
                    if (!string.IsNullOrWhiteSpace(rawText))
                    {
                        string cleanedText = _cleaner.CleanContactInfo(rawText);
                        cleanedParagraphs.Add(cleanedText);
                    }
                }
            }
        }

        return cleanedParagraphs;
    }

    private class ParsedPaperModel
    {
        public int Order { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Paragraphs { get; set; } = new();
    }
}