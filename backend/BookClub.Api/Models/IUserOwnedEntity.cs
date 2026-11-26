namespace BookClub.Api.Models;

public interface IUserOwnedEntity
{
    int Id { get; }
    int UserId { get; }
}
