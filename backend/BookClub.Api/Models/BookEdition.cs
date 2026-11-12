namespace BookClub.Api.Models;

public class BookEdition
{
    public int EditionId { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public int TotalPageCount { get; set; }
    public string? Publisher { get; set; }
    public string? PublishDate { get; set; }
    public string? CoverUrl { get; set; }
}