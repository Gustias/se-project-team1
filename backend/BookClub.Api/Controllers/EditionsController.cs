using BookClub.Api.DTOs;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[ApiController]
[Route("api/editions")]
public class EditionsController : ControllerBase
{
    private readonly OpenLibraryService _openLibraryService;

    public EditionsController(
        OpenLibraryService openLibraryService)
    {
        _openLibraryService = openLibraryService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string workId,
        CancellationToken cancellationToken)
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

        var editions = await _openLibraryService.SearchEditionsAsync(
            id,
            cancellationToken);

        if (editions is null)
        {
            return NotFound();
        }

        return Ok(editions);
    }
}
    