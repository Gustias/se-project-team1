using System.ComponentModel.DataAnnotations;
using BookClub.Api.Dtos;

namespace BookClub.Api.Tests;

public class LoginValidationTests
{
    private static (bool IsValid, List<ValidationResult> Results)
        ValidateRequest(LoginRequest request)
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
    public void LoginRequest_WhenAllFieldsAreValid_PassesValidation()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void LoginRequest_WhenEmailIsEmpty_FailsValidation()
    {
        var request = new LoginRequest
        {
            Email = "",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.False(isValid);
        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(LoginRequest.Email)));
    }

    [Fact]
    public void LoginRequest_WhenEmailIsInvalid_FailsValidation()
    {
        var request = new LoginRequest
        {
            Email = "invalid-email",
            Password = "Password123"
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.False(isValid);
        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(LoginRequest.Email)));
    }

    [Fact]
    public void LoginRequest_WhenPasswordIsEmpty_FailsValidation()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        var (isValid, results) = ValidateRequest(request);

        Assert.False(isValid);
        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(LoginRequest.Password)));
    }
}