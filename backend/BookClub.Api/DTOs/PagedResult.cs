namespace BookClub.Api.DTOs;

public class PagedResult<T>
{
    // Total number of items available, not just the ones in this page.
    public int Total { get; set; }
    public List<T> Items { get; set; } = new();
}
