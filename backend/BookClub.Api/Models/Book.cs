namespace BookClub.Api.Models;

public class Book
{
    public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();
    public ICollection<ReadingLog> ReadingLogs { get; set; } = new List<ReadingLog>();
    public ICollection<BookRatingEntry> Ratings { get; set; } = new List<BookRatingEntry>();
    public ICollection<BookEdition> Editions { get; set; } = new List<BookEdition>();
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
    public string? DefaultCoverUrl { get; set; }
}
