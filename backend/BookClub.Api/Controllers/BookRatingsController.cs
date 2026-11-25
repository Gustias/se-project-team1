using BookClub.Api.DTOs;
using BookClub.Api.Services;
using BookClub.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/book-ratings")]
public class BookRatingsController : ControllerBase
{
    private readonly BookRatingService _bookRatingService;

    public BookRatingsController(
        BookRatingService bookRatingService)
    {
        _bookRatingService = bookRatingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateBookRatingDto request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new
            {
                message = "Rating must be between 1 and 5"
            });
        }

        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var rating = await _bookRatingService.CreateAsync(
                currentUserId.Value,
                request.BookId,
                request.Rating);

            if (rating is null)
            {
                return Conflict(new
                {
                    message = "Could not create rating. Check whether the user and book exist, the book is marked as read, and the book has not already been rated."
                });
            }

            return Ok(rating);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new
            {
                message = "Rating must be between 1 and 5."
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var rating = await _bookRatingService.GetAsync(id, currentUserId.Value);

        if (rating is null)
        {
            return NotFound();
        }

        return Ok(rating);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAsync()
    {
        var currentUserId = User.GetUserId();

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var list = await _bookRatingService.GetListAsync(currentUserId.Value);

        return Ok(list);
    }
}
