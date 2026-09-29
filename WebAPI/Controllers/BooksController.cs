using Application.Features.Books.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator) => _mediator = mediator;

    [HttpPost("generate")]
    public async Task<IActionResult> CreateBook([FromForm] string bookName, [FromForm] List<IFormFile> files)
    {
        try
        {
            var command = new CreateBookCommand(bookName, files);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}