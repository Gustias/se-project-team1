namespace BookClub.Api.DTOs;

public class GetReadingProgressDto
{
    public required int Id { get; set; }
    public required int UserId{ get; set; }
    public required int BookId{ get; set; }
    public int Progress{ get; set; }
    public int? Chapter{ get; set; }
    public DateOnly DateRead{ get; set; }
}