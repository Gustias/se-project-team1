namespace BookClub.Api.Models;

public class BookRatingEntry
{
    public int Id { get; set; }

    // User who submitted the rating
    public required int UserId { get; set; }
    public User User { get; set; } = null!;

    // Book being rated
    public required int BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Optional specific edition
    public int? BookEditionId { get; set; }
    public BookEdition? BookEdition { get; set; }

    // Rating information
    public int Rating { get; set; }
    public string? Review { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}