using System.ComponentModel.DataAnnotations;

namespace BookClub.Api.DTOs;

public class UpdateReadingProgressDto
{
    [Range(0, 100)] public int Progress { get; set; }
    public int? Chapter { get; set; }
}