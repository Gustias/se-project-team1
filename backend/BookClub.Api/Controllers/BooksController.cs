using BookClub.Api.DTOs;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;
    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Search query cannot be empty.");
        }

        var books = await _bookService.SearchBooksAsync(q, cancellationToken);
        return Ok(books);
    }
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        CreateBookDto request)
    {
        var entry = await _bookService.CreateAsync(
            request.ExternalId,
            request.Isbn,
            request.Title,
            request.Author);
        
        if (entry is null)
        {
            return Conflict(new
            {
                message = "Error creating a book."
            });
        }
        
        return CreatedAtAction(nameof(GetAsync), new { id = entry.Id}, entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var deleted = await _bookService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var entry = await _bookService.GetAsync(id);

        if (entry is null)
        {
            return NotFound();
        }

        return Ok(entry);
    }
    
}