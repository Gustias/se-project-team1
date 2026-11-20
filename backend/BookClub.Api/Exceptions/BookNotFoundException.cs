namespace BookClub.Api.Exceptions;

public sealed class BookNotFoundException : DomainException
{
    public BookNotFoundException(int bookId)
        : base($"Book with ID {bookId} was not found.")
    {
    }
}