using System.ComponentModel.DataAnnotations;
using BookClub.Api.Dtos;

namespace BookClub.Api.Tests;

public class RegistrationValidationTests
{
    private static (bool IsValid, List<ValidationResult> Results)
        ValidateRequest(RegisterRequest request)
    {
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        return (isValid, results);
    }

    [Fact]
    public void RegisterRequest_WhenAllFieldsAreValid_PassesValidation()
    {
        var request = new RegisterRequest
        {
            UserName = "TestUser",
            Email = "test@example.com",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void RegisterRequest_WhenUserNameIsEmpty_FailsValidation()
    {
        var request = new RegisterRequest
        {
            UserName = "",
            Email = "test@example.com",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.False(isValid);
        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(RegisterRequest.UserName)));
    }

    [Fact]
    public void RegisterRequest_WhenEmailIsInvalid_FailsValidation()
    {
        var request = new RegisterRequest
        {
            UserName = "TestUser",
            Email = "invalid-email",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.False(isValid);
        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Fact]
    public void RegisterRequest_WhenEmailIsEmpty_FailsValidation()
    {
        var request = new RegisterRequest
        {
            UserName = "TestUser",
            Email = "",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.False(isValid);
        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Fact]
    public void RegisterRequest_WhenPasswordIsEmpty_FailsValidation()
    {
        var request = new RegisterRequest
        {
            UserName = "TestUser",
            Email = "test@example.com",
            Password = ""
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.False(isValid);
        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(50, true)]
    [InlineData(51, false)]
    public void RegisterRequest_WhenUserNameHasBoundaryLength_ValidatesCorrectly(
        int length, bool expected)
    {
        var request = new RegisterRequest
        {
            UserName = new string('x', length),
            Email = "test@example.com",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.Equal(expected, isValid);

        if (!expected)
        {
            Assert.Contains(results, r =>
                r.MemberNames.Contains(nameof(RegisterRequest.UserName)));
        }
        else
        {
            Assert.Empty(results);
        }
    }
}
