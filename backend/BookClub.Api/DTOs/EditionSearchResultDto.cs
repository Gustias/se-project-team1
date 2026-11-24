namespace BookClub.Api.DTOs;

public class EditionSearchResultDto
{
    public string ExternalId { get; set; } = default!;
    public string WorkExternalId { get; set; } = default!;
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public string Title { get; set; } = default!;
    public string? Subtitle { get; set; }
    public List<string>? Publishers { get; set; }
    public string? PublishDate { get; set; }
    public int? PageCount { get; set; }
    public string? Format { get; set; }
    public string? Language { get; set; }
    public string? CoverUrl { get; set; }
}