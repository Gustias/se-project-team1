namespace BookClub.Api.DTOs;

public class WorkDetailsDto
{
    public string ExternalId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public string? FirstPublishDate { get; set; }
}
