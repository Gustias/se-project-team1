using Microsoft.AspNetCore.Identity;

namespace BookClub.Api.Models;

public class User : IdentityUser<int>
{
    public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();
    public ICollection<ReadingLog> ReadingLogs { get; set; } = new List<ReadingLog>();
    public ICollection<BookRatingEntry> BookRatings { get; set; } = new List<BookRatingEntry>();
}
