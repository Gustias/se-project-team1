using BookClub.Api.DTOs;
using BookClub.Api.Models;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[ApiController]
[Route("api/reading-log")]
public class ReadingLogController : ControllerBase
{
    private readonly ReadingLogService _readingLogService;

    public ReadingLogController(ReadingLogService readingLogService)
    {
        _readingLogService = readingLogService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry(CreateReadingLogDto request)
    {

        if (request.StartPage > request.EndPage)
        {
            return BadRequest("EndPage must be greater than or equal to StartPage");
        }

        var entry =  await _readingLogService.CreateAsync(
            request.UserId,
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

    [HttpGet("ById/{id}")]
    public async Task<IActionResult> GetEntryById(int id)
    {
        var log = await _readingLogService.GetAsync(id);

        if (log is null)
        {
            return NotFound();
        }

        return Ok(log);
    }

    [HttpGet("summary/{userId}")]
    public async Task<IActionResult> GetSummary(int userId,
        DateOnly from, DateOnly to)
    {
        return Ok();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var deleted = await _readingLogService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }


}
