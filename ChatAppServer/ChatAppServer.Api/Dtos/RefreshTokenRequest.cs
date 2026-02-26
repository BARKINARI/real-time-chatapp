using System.ComponentModel.DataAnnotations;

namespace ChatAppServer.Api.Dtos;

public record class RefreshTokenRequest(
    Guid Id,
    [Required] string RefreshToken
);