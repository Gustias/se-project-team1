using System.ComponentModel.DataAnnotations;

namespace BookClub.Api.Dtos;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string UserName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}