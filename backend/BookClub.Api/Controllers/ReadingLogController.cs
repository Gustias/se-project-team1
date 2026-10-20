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
    public async Task<IActionResult> CreateAsync(CreateReadingLogDto request)
    {
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        return NoContent();
    }


}
