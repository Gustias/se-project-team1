using BookClub.Api.DTOs;

namespace BookClub.Api.Services;

public record CreateBookResult(
    GetBookDto Book,
    bool Created);