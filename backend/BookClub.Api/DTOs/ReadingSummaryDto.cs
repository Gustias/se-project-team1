namespace BookClub.Api.DTOs;

public class DailyReadingDto
{
    public DateOnly Date { get; set; }
    public int PagesRead { get; set; }
}

public class ReadingSummaryDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalPages { get; set; }
    public List<DailyReadingDto> Days { get; set; } = new();
}
