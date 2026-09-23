using BookClub.Api.DTOs;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

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
        var entry = await _readingProgressService.CreateAsync(
            request.UserId,
            request.BookId,
            request.Progress,
            request.Chapter);

        if (entry is null)
        {
            return Conflict(new
            {
                message = "Reading progress for this user and book already exists."
            });
        }

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
        var updated = await _readingProgressService.UpdateAsync(
            id,
            request.Progress,
            request.Chapter);

        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var deleted = await _readingProgressService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntry(int id)
    {
        var entry = await _readingProgressService.GetAsync(id);

        if (entry is null)
        {
            return NotFound();
        }

        return Ok(entry);
    }
}