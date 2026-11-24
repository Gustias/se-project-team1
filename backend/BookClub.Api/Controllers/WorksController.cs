using BookClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[ApiController]
[Route("api/works")]
public class WorksController : ControllerBase
{
    private readonly OpenLibraryService _openLibraryService;

    public WorksController(
        OpenLibraryService openLibraryService)
    {
        _openLibraryService = openLibraryService;
    }

    [HttpGet("{workId}")]
    public async Task<IActionResult> Get(
        string workId,
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

        try
        {
            var work = await _openLibraryService.GetWorkAsync(
                id,
                cancellationToken);

            if (work is null)
            {
                return NotFound();
            }

            return Ok(work);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Could not load the book from Open Library. Please try again later.");
        }
    }
}
