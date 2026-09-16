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
}