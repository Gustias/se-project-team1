namespace BookClub.Api.DTOs;

public class ReadingLogDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public int PagesRead => EndPage - StartPage;
    public DateOnly Date { get; set; }
}
