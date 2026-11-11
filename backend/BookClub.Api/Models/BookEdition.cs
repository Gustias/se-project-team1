namespace BookClub.Api.Models;

public class BookEdition : Book
{
    public int EditionId { get; set; }
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
}