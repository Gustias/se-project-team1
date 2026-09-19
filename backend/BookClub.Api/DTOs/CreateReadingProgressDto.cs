using System.ComponentModel.DataAnnotations;

namespace BookClub.Api.DTOs;

public class CreateReadingProgressDto
{
    public required int UserId { get; set; }
    public required int BookId { get; set; }
    [Range(0, 100)] public int Progress { get; set; }
    public int? Chapter { get; set; }
}