using BookClub.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Search query cannot be empty.");
        }
        var books = _bookService.SearchBooks(q);
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
        
        return Created(
            $"/api/books/{entry.ExternalId}",
            entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        
    }
    
}