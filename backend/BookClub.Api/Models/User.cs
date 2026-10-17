namespace BookClub.Api.Models;

public class User
{
    public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();
    public ICollection<ReadingLog> ReadingLogs { get; set; } = new List<ReadingLog>();
    public ICollection<BookRatingEntry> BookRatings { get; set; } = new List<BookRatingEntry>();
    public int Id { get; set; }
    public required string UserName { get; set; }
}
