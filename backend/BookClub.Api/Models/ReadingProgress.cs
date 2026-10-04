namespace BookClub.Api.Models;

public enum ReadingStatus
{
    Reading,
    Read,
    WantToRead
}

public class ReadingProgress
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required int BookId { get; set; }
    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
    public int Progress { get; set; }
    public string? LastChapterRead { get; set; }
    public DateOnly? DateRead { get; set; }
    public ReadingStatus Status { get; set; }
}
