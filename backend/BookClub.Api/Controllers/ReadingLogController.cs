using BookClub.Api.DTOs;
using BookClub.Api.Models;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BookClub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reading-log")]
public class ReadingLogController : ControllerBase
{
    private readonly ReadingLogService _readingLogService;
    private readonly UserManager<User> _userManager;

    public ReadingLogController(
        ReadingLogService readingLogService,
        UserManager<User> userManager)
    {
        _readingLogService = readingLogService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry(CreateReadingLogDto request)
    {

        if (request.StartPage > request.EndPage)
        {
            return BadRequest("EndPage must be greater than or equal to StartPage");
        }

        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var entry = await _readingLogService.CreateAsync(
            currentUserId,
            request.BookId,
            request.StartPage,
            request.EndPage,
            request.Date
            );

        if (entry is null)
        {
            return BadRequest("Could not create a reading log.");
        }

        return CreatedAtAction(nameof(GetEntryById), new { id = entry.Id }, entry);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEntryById(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var log = await _readingLogService.GetAsync(id, currentUserId);

        if (log is null)
        {
            return NotFound();
        }

        return Ok(log);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(DateOnly from, DateOnly to)
    {

        if (to < from)
        {
            return BadRequest("'to' date must be on or after 'from' date.");
        }

        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        return Ok(await _readingLogService.GetSummaryAsync(currentUserId, from, to));
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var deleted = await _readingLogService.DeleteAsync(id, currentUserId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

}
