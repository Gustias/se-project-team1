using System.ComponentModel.DataAnnotations;

namespace BookClub.Api.DTOs;

public class CreateBookEditionDto : CreateBookDto
{
    [Required(AllowEmptyStrings = false), MaxLength(100)]
    public required string WorkExternalId { get; set; }
    [MaxLength(20)]
    public string? Isbn10 { get; set; }
    [MaxLength(20)]
    public string? Isbn13 { get; set; }
    [Range(1, 100_000)]
    public int TotalPageCount { get; set; }
    [MaxLength(500)]
    public string? Publisher { get; set; }
    // restriction?
    public string? PublishDate { get; set; }
    public string? CoverUrl { get; set; }
}