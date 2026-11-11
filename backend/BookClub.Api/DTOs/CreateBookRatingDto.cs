namespace BookClub.Api.DTOs;

public class CreateBookRatingDto
{
    public required int BookId { get; set; }
    public int? BookEditionId { get; set; }
    public required int Rating { get; set; }
    public string? Review { get; set; }
}