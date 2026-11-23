using BookClub.Api.DTOs;
using BookClub.Api.Services;
using BookClub.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BookClub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reading-progress")]
public class ReadingProgressController : ControllerBase
{
    private readonly ReadingProgressService _readingProgressService;
    private readonly UserManager<User> _userManager;

    public ReadingProgressController(
        ReadingProgressService readingProgressService,
        UserManager<User> userManager)
    {
        _readingProgressService = readingProgressService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry(
        CreateReadingProgressDto request)
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var entry = await _readingProgressService.CreateAsync(
            currentUserId,
            request.BookId,
            request.Progress,
            request.LastChapterRead,
            request.Status);

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
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var updated = await _readingProgressService.UpdateAsync(
            id,
            currentUserId,
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
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var deleted = await _readingProgressService.DeleteAsync(
            id,
            currentUserId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntry(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var entry = await _readingProgressService.GetAsync(
            id,
            currentUserId);

        if (entry is null)
        {
            return NotFound();
        }

        return Ok(entry);
    }
}
