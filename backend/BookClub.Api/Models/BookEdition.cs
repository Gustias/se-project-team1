namespace BookClub.Api.Models;

public class BookEdition : Book
{
    public int EditionId { get; set; }
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public int TotalPageCount { get; set; }
    public string? Publisher { get; set; }
    public string? PublishDate { get; set; }
    public string? CoverUrl { get; set; }
}