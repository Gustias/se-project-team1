using BookClub.Api.DTOs;
using BookClub.Api.Services;
using BookClub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
namespace BookClub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/book-ratings")]
public class BookRatingsController : ControllerBase
{
    private readonly BookRatingService _bookRatingService;
    private readonly UserManager<User> _userManager;

    public BookRatingsController(
        BookRatingService bookRatingService,
        UserManager<User> userManager)
    {
        _bookRatingService = bookRatingService;
        _userManager = userManager;
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

        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        try
        {
            var rating = await _bookRatingService.CreateAsync(
                currentUserId,
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
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var currentUserId))
        {
            return Unauthorized();
        }

        var rating = await _bookRatingService.GetAsync(id, currentUserId);

        if (rating is null)
        {
            return NotFound();
        }

        return Ok(rating);
    }
}