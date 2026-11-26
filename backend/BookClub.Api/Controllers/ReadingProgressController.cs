using BookClub.Api.DTOs;
using BookClub.Api.Extensions;
using BookClub.Api.Models;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reading-progress")]
public class ReadingProgressController : ControllerBase
{
    private readonly ReadingProgressService _readingProgressService;

    public ReadingProgressController(
        ReadingProgressService readingProgressService)
    {
        _readingProgressService = readingProgressService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry(
        CreateReadingProgressDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entry =
            await _readingProgressService.CreateAsync(
                currentUserId.Value,
                request.BookId,
                request.Progress,
                request.LastChapterRead,
                request.Status,
                request.TotalPages,
                request.PagesRead,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetEntry),
            new { id = entry.Id },
            entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntry(
        int id,
        UpdateReadingProgressDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var updated =
            await _readingProgressService.UpdateAsync(
                id,
                currentUserId.Value,
                request.Progress,
                request.LastChapterRead,
                request.Status,
                request.TotalPages,
                request.PagesRead,
                cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
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
            await _readingProgressService.DeleteAsync(
                id,
                currentUserId.Value,
                cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("by-work/{workExternalId}")]
    public async Task<IActionResult> GetEntryByWork(
        string workExternalId,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entry =
            await _readingProgressService.GetByWorkAsync(
                workExternalId,
                currentUserId.Value,
                cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        return Ok(entry);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntry(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entry =
            await _readingProgressService.GetAsync(
                id,
                currentUserId.Value,
                cancellationToken);

        if (entry is null)
        {
            return NotFound();
        }

        return Ok(entry);
    }

    [HttpGet]
    public async Task<IActionResult> GetEntries(
        CancellationToken cancellationToken,
        [FromQuery] ReadingStatus? status = null)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entries =
            await _readingProgressService.GetLibraryAsync(
                currentUserId.Value,
                status,
                cancellationToken);

        return Ok(entries);
    }
}
