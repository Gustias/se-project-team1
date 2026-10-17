namespace BookClub.Api.Models;

public readonly record struct BookRating : IComparable<BookRating>
{
    public int Value { get; }

    public BookRating(int value)
    {
        if (value < 1 || value > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Rating must be between 1 and 5.");
        }

        Value = value;
    }

    public int CompareTo(BookRating other)
    {
        return Value.CompareTo(other.Value);
    }
}