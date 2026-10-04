namespace BookClub.Api.DTOs;

public class GetBookDto
{
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
}
