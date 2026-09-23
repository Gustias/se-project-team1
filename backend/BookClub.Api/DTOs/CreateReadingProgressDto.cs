using System.ComponentModel.DataAnnotations;

namespace BookClub.Api.DTOs;

public class CreateReadingProgressDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Range(0, 100)]
    public int Progress { get; set; }

    public int? Chapter { get; set; }
}