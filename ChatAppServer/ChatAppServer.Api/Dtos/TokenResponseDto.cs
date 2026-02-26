using System.ComponentModel.DataAnnotations;

namespace ChatAppServer.Api.Dtos;

public record TokenResponseDto(
    [Required] string AccessToken,
    [Required] string RefreshToken
);
