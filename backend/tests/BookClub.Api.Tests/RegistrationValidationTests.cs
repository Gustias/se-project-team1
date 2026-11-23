using System.ComponentModel.DataAnnotations;
using BookClub.Api.Dtos;

namespace BookClub.Api.Tests;

public class RegistrationValidationTests
{
    [Fact]
    public void RegisterRequest_WhenAllFieldsAreValid_PassesValidation()
    {
        var request = new RegisterRequest
        {
            UserName = "TestUser",
            Email = "test@example.com",
            Password = "Password123"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            context,
            results,
            validateAllProperties: true
        );

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void RegisterRequest_WhenUserNameIsTooShort_FailsValidation()
    {
        var request = new RegisterRequest
        {
            UserName = "Us",
            Email = "test@example.com",
            Password = "Password123"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            context,
            results,
            validateAllProperties: true
        );

        Assert.False(isValid);
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(RegisterRequest.UserName)));
    }

    [Fact]
    public void RegisterRequest_WhenUserNameIsTooLong_FailsValidation()
    {
        var request = new RegisterRequest
        {
            UserName = new string('X', 51),
            Email = "test@example.com",
            Password = "Password123"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            context,
            results,
            validateAllProperties: true
        );

        Assert.False(isValid);
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(RegisterRequest.UserName)));
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

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            context,
            results,
            validateAllProperties: true
        );

        Assert.False(isValid);
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(RegisterRequest.UserName)));
    }
}