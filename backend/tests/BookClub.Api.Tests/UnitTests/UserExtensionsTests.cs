using System.Security.Claims;
using BookClub.Api.Extensions;

namespace BookClub.Api.Tests;

public class UserExtensionsTests
{
    private static ClaimsPrincipal CreateUser(string? userId = null)
    {
        var identity = new ClaimsIdentity("TestAuthentication");

        if (userId is not null)
        {
            identity.AddClaim(
                new Claim(ClaimTypes.NameIdentifier, userId));
        }

        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void GetUserId_WhenIdIsValid_ReturnsId()
    {
        var user = CreateUser("5");

        var result = user.GetUserId();

        Assert.Equal(5, result);
    }

    [Fact]
    public void GetUserId_WhenIdIsMissing_ReturnsNull()
    {
        var user = CreateUser();

        var result = user.GetUserId();

        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_WhenIdIsInvalid_ReturnsNull()
    {
        var user = CreateUser("abc");

        var result = user.GetUserId();

        Assert.Null(result);
    }
}