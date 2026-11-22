using BookClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[ApiController]
[Route("api/editions")]
public class EditionsController : ControllerBase
{
    private const int MaxLimit = 50;

    private readonly OpenLibraryService _openLibraryService;

    public EditionsController(
        OpenLibraryService openLibraryService)
    {
        _openLibraryService = openLibraryService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string workId,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(workId))
        {
            return BadRequest("Work id cannot be empty.");
        }

        var id = workId.Trim();

        if (id.Length > 50)
        {
            return BadRequest("Work id must be 50 characters or fewer.");
        }

        if (limit < 1 || limit > MaxLimit)
        {
            return BadRequest($"Limit must be between 1 and {MaxLimit}.");
        }

        if (offset < 0)
        {
            return BadRequest("Offset cannot be negative.");
        }

        try
        {
            var editions = await _openLibraryService.SearchEditionsAsync(
                id,
                cancellationToken,
                limit,
                offset);

            if (editions is null)
            {
                return NotFound();
            }

            return Ok(editions);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Could not load editions from Open Library. Please try again later.");
        }
    }
}
