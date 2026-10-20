namespace BookClub.Api.Models;

public class BookRatingEntry
{
    public int Id { get; set; }

    public required int UserId { get; set; }
    public required int BookId { get; set; }

    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
    public BookRating Rating { get; set; }
}