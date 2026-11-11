namespace BookClub.Api.DTOs;

public class GetBookEditionDto : GetBookDto
{
    public required string WorkExternalId { get; set; }
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public int TotalPageCount { get; set; }
    public string? Publisher { get; set; }
    public string? PublishDate { get; set; }
    public string? CoverUrl { get; set; }
}