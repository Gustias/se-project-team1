namespace BookClub.Api.Models;

public class ReadingLog
{
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
    public required int UserId { get; set; }
    public required int BookId { get; set; }
    public int? PagesRead { get; set; }
    public DateOnly Date { get; set; }
}