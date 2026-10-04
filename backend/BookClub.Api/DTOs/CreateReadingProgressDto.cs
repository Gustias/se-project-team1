using System.ComponentModel.DataAnnotations;
using BookClub.Api.Models;

namespace BookClub.Api.DTOs;

public class CreateReadingProgressDto
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Range(0, 100)]
    public int Progress { get; set; }

    [MaxLength(200)]
    public string? LastChapterRead { get; set; }

    [EnumDataType(typeof(ReadingStatus))]
    public ReadingStatus Status { get; set; }
}
