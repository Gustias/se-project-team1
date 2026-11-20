namespace BookClub.Api.Exceptions;

public sealed class ReadingProgressAlreadyExistsException : DomainException
{
    public ReadingProgressAlreadyExistsException()
        : base("Reading progress for this user and book already exists.")
    {
    }
}