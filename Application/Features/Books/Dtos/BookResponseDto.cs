namespace Application.Features.Books.Dtos;

public record BookResponseDto(
    Guid Id,
    string BookTitle,
    string Status,
    string? PdfUrl,
    string? ErrorMessage
);