using System.ComponentModel.DataAnnotations;

namespace BookClub.Api.DTOs;

public class CreateReadingLogDto
{
    public required int UserId { get; set; }
    public required int BookId { get; set; }
    [Range(0, int.MaxValue)]
    public int StartPage { get; set; }
    [Range(0, int.MaxValue)]
    public int EndPage { get; set; }
    public required DateOnly Date { get; set; }
}
