namespace BookClub.Api.Models;

public class Book
{
    public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();
    public ICollection<ReadingLog> ReadingLogs { get; set; } = new List<ReadingLog>();
    public int Id { get; set; }
    public string? Isbn { get; set; }
    public required string ExternalId { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
}