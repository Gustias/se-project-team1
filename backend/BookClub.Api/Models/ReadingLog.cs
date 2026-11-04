namespace BookClub.Api.Models;

public class ReadingLog
{
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public Book Book { get; set; } = null!;
    public required int UserId { get; set; }
    public required int BookId { get; set; }
    public int StartPage { get; set; } // page that user started on during the session
    public int EndPage { get; set; }  // page that user ended up on during the session
    public required DateOnly Date { get; set; }
}
