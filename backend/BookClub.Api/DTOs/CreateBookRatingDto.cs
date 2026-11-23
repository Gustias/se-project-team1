namespace BookClub.Api.DTOs;

public class CreateBookRatingDto
{
    public required int BookId { get; set; }
    public required int Rating { get; set; }
}