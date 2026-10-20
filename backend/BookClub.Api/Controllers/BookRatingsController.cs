using BookClub.Api.DTOs;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Mvc;
namespace BookClub.Api.Controllers;

[ApiController]
[Route("api/book-ratings")]
public class BookRatingsController : ControllerBase
{
    private readonly BookRatingService _bookRatingService;

    public BookRatingsController(BookRatingService bookRatingService)
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

        try
        {
            var rating = await _bookRatingService.CreateAsync(
                request.UserId,
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
        var rating = await _bookRatingService.GetAsync(id);

        if (rating is null)
        {
            return NotFound();
        }

        return Ok(rating);
    }
}