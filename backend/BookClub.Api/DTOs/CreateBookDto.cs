namespace BookClub.Api.DTOs;

public class CreateBookDto
{
    public required string ExternalId { get; set; }
    public string? Isbn { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
}