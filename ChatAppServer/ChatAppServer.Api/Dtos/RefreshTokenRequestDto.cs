using System.ComponentModel.DataAnnotations;

namespace ChatAppServer.Api.Dtos;

public record RefreshTokenRequestDto(
    Guid UserId,
    [Required] string RefreshToken
);