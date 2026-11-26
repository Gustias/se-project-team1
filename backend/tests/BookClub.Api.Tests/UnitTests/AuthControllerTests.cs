using System.Text.Json;
using BookClub.Api.Controllers;
using BookClub.Api.Dtos;
using BookClub.Api.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookClub.Api.Tests;

public class AuthControllerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();

        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);

        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null, null, null, null);

        _controller = new AuthController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            Mock.Of<IAntiforgery>());
    }

    private static void AssertMessage(
        ObjectResult response, string expectedMessage)
    {
        var body = JsonSerializer.SerializeToElement(response.Value);

        Assert.Equal(
            expectedMessage,
            body.GetProperty("Message").GetString());
    }

    private void VerifyUserLookup(LoginRequest request)
    {
        _userManagerMock.Verify(
            manager => manager.FindByEmailAsync(request.Email),
            Times.Once());
    }

    private void VerifySignIn(User user, LoginRequest request)
    {
        _signInManagerMock.Verify(
            manager => manager.PasswordSignInAsync(
                user, request.Password, true, true),
            Times.Once());
    }

    [Fact]
    public async Task Login_WhenUserDoesNotExist_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "unknown@example.com",
            Password = "Password123"
        };

        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var result = await _controller.Login(request);

        var response = Assert.IsType<UnauthorizedObjectResult>(result);

        AssertMessage(response, "Invalid email or password.");
        VerifyUserLookup(request);

        _signInManagerMock.Verify(
            manager => manager.PasswordSignInAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Never());
    }

    [Fact]
    public async Task Login_WhenPasswordIsIncorrect_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123"
        };

        var user = new User
        {
            UserName = "TestUser",
            Email = request.Email
        };

        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(manager => manager.PasswordSignInAsync(
                user, request.Password, true, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await _controller.Login(request);

        var response = Assert.IsType<UnauthorizedObjectResult>(result);

        AssertMessage(response, "Invalid email or password.");
        VerifyUserLookup(request);
        VerifySignIn(user, request);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsOk()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        var user = new User
        {
            UserName = "TestUser",
            Email = request.Email
        };

        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(manager => manager.PasswordSignInAsync(
                user, request.Password, true, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var result = await _controller.Login(request);

        var response = Assert.IsType<OkObjectResult>(result);

        AssertMessage(response, "Logged in successfully.");
        VerifyUserLookup(request);
        VerifySignIn(user, request);
    }
}
