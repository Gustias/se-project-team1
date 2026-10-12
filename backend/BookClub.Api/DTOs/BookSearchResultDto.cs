public class BookSearchResultDto
{
    public string ExternalId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? CoverUrl { get; set; }
    public int? Year { get; set; }
    public string? Isbn10 { get; set; }
    public string? Isbn13 { get; set; }
    public List<string> Publishers { get; set; } = new();
}
