
namespace BookClub.Api.Models;

public class BookRatingEntry
{
    public int Id { get; set; }

    // Required relationship to the book
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Optional relationship to a specific edition
    public int? BookEditionId { get; set; }
    public BookEdition? BookEdition { get; set; }

    public int Rating { get; set; }
    public string? Review { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}