using System.ComponentModel.DataAnnotations;
using BookClub.Api.Models;

namespace BookClub.Api.DTOs;

public class UpdateReadingProgressDto
{
    [Range(0, 100)]
    public int Progress { get; set; }
    [MaxLength(200)]
    public string? LastChapterRead { get; set; }
    [EnumDataType(typeof(ReadingStatus))]
    public ReadingStatus Status { get; set; }
}
