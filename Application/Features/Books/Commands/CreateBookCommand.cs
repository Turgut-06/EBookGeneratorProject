using Application.Features.Books.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Books.Commands;

public record CreateBookCommand(
    string bookName,
    List<IFormFile> files
) : IRequest<BookResponseDto>;