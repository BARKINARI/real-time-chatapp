using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChatAppServer.Api.Data;
using ChatAppServer.Api.Dtos;
using ChatAppServer.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ChatAppServer.Api.Services;

public class AuthService(ApplicationDbContext db, IConfiguration configuration) : IAuthService
{
    public async Task<TokenResponseDto?> LoginAsync(AuthUserDto request)
    {
        var user = await db.AuthUsers.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null)
        {
            return null;
        }

        if (new PasswordHasher<AuthUser>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await CreateTokenResponse(user);
    }

    private async Task<TokenResponseDto> CreateTokenResponse(AuthUser user)
    {
        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }

    public async Task<AuthUser?> RegisterAsync(AuthUserDto request)
    {
        if (await db.AuthUsers.AnyAsync(u => u.Username == request.Username))
        {
            return null;
        }

        var user = new AuthUser();

        var hashedPassword = new PasswordHasher<AuthUser>()
            .HashPassword(user, request.Password);

        user.Username = request.Username;
        user.PasswordHash = hashedPassword;

        db.AuthUsers.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);

        if (user is null)
        {
            return null;
        }

        return await CreateTokenResponse(user);
    }

    private async Task<AuthUser?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var user = await db.AuthUsers.FindAsync(userId);
        if (user is null || user.RefreshToken != refreshToken 
            || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }
        return user;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(AuthUser user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await db.SaveChangesAsync();
        return refreshToken;
    }

    private string CreateToken(AuthUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetValue<String>("AppSettings:Token")!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
