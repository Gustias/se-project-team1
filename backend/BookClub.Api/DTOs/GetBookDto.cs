namespace BookClub.Api.DTOs;

public class GetBookDto 
{
    public int Id { get; set; }
    public required string ExternalId { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
    public string? DefaultCoverUrl { get; set; }
}
