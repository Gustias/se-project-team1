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
}