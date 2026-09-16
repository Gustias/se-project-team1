public class BookService
{
    public List<BookSearchResultDto> SearchBooks(string query)
    {
        return new List<BookSearchResultDto>
        {
            new BookSearchResultDto
            {
                ExternalId = "test-1",
                Title = "Dune",
                Author = "Frank Herbert",
                CoverUrl = null
            }
        };
    }
}