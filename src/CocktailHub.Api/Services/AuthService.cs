using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CocktailHub.Api.DTOs.Auth;
using CocktailHub.Core.Entities;
using CocktailHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CocktailHub.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
            return null;

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "default-secret-key-min-32-chars!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "CocktailHub",
            audience: _config["Jwt:Audience"] ?? "CocktailHub",
            claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );
        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email,
            user.Role.ToString()
        );
    }
}
