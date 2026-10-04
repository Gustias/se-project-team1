using BookClub.Api.Models;

namespace BookClub.Api.DTOs;

public class GetReadingProgressDto
{
    public required int Id { get; set; }
    public required int UserId { get; set; }
    public required int BookId { get; set; }
    public int Progress { get; set; }
    public string? LastChapterRead { get; set; }
    public DateOnly? DateRead { get; set; }
    public ReadingStatus Status { get; set; }
}
