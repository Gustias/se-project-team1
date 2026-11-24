using BookClub.Api.DTOs;
using BookClub.Api.Services;
using BookClub.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        CreateReadingProgressDto request)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entry = await _readingProgressService.CreateAsync(
            currentUserId.Value,
            request.BookId,
            request.Progress,
            request.LastChapterRead,
            request.Status);

        return CreatedAtAction(
            nameof(GetEntry),
            new { id = entry.Id },
            entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntry(
        int id,
        UpdateReadingProgressDto request)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var updated = await _readingProgressService.UpdateAsync(
            id,
            currentUserId.Value,
            request.Progress,
            request.LastChapterRead,
            request.Status);

        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var deleted = await _readingProgressService.DeleteAsync(
            id,
            currentUserId.Value);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntry(int id)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entry = await _readingProgressService.GetAsync(
            id,
            currentUserId.Value);

        if (entry is null)
        {
            return NotFound();
        }

        return Ok(entry);
    }
}
