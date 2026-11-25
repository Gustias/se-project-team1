using BookClub.Api.Models;

namespace BookClub.Api.DTOs;

public class ReadingListItemDto
{
    public required int Id { get; set; }
    public required int BookId { get; set; }
    public required string ExternalId { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn13 { get; set; }
    public ReadingStatus Status { get; set; }
    public int Progress { get; set; }
    public int PagesRead { get; set; }
    public int? TotalPages { get; set; }
    public string? LastChapterRead { get; set; }
}