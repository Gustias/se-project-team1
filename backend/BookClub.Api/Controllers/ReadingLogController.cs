using BookClub.Api.DTOs;
using BookClub.Api.Extensions;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reading-log")]
public class ReadingLogController : ControllerBase
{
    private readonly ReadingLogService _readingLogService;

    public ReadingLogController(
        ReadingLogService readingLogService)
    {
        _readingLogService = readingLogService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry(
        CreateReadingLogDto request,
        CancellationToken cancellationToken)
    {
        if (request.StartPage > request.EndPage)
        {
            return BadRequest(
                "EndPage must be greater than or equal to StartPage");
        }

        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entry =
            await _readingLogService.CreateAsync(
                currentUserId.Value,
                request.BookId,
                request.StartPage,
                request.EndPage,
                request.Date,
                cancellationToken);

        if (entry is null)
        {
            return BadRequest(
                "Could not create a reading log.");
        }

        return CreatedAtAction(
            nameof(GetEntryById),
            new { id = entry.Id },
            entry);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntryById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var log =
            await _readingLogService.GetAsync(
                id,
                currentUserId.Value,
                cancellationToken);

        if (log is null)
        {
            return NotFound();
        }

        return Ok(log);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        if (to < from)
        {
            return BadRequest(
                "'to' date must be on or after 'from' date.");
        }

        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var summary =
            await _readingLogService.GetSummaryAsync(
                currentUserId.Value,
                from,
                to,
                cancellationToken);

        return Ok(summary);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var deleted =
            await _readingLogService.DeleteAsync(
                id,
                currentUserId.Value,
                cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}