using ChatAppServer.Api.Dtos;
using ChatAppServer.Api.Models;

namespace ChatAppServer.Api.Services;

public interface IAuthService
{
    Task<AuthUser?> RegisterAsync(AuthUserDto request);
    Task<TokenResponseDto?> LoginAsync(AuthUserDto request);
    Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
}
