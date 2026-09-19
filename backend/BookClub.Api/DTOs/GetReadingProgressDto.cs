namespace BookClub.Api.DTOs;

public class GetReadingProgressDto
{
    public required int Id;
    public required int UserId;
    public required int BookId;
    public int Progress;
    public int? Chapter;
    public DateOnly DateRead;
}