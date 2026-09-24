using System.ComponentModel.DataAnnotations;

namespace BookClub.Api.DTOs;

public class CreateBookDto
{
    [Required(AllowEmptyStrings = false), MaxLength(100)]
    public required string ExternalId { get; set; }
    [MaxLength(20)]
    public string? Isbn { get; set; }
    [Required(AllowEmptyStrings = false), MaxLength(500)]
    public required string Title { get; set; }
    [MaxLength(200)]
    public string? Author { get; set; }
}