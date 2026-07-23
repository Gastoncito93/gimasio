using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services.Business;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<(bool Success, LoginResponseDto? Data, string? Error)> LoginAsync(LoginRequestDto dto)
    {
        var user = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.Trim().ToLower());

        if (user == null)
        {
            return (false, null, "Credenciales incorrectas.");
        }

        if (user.EliminadoAt.HasValue)
        {
            return (false, null, "El usuario está deshabilitado.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return (false, null, "Credenciales incorrectas.");
        }

        var token = GenerateJwtToken(user);

        var response = new LoginResponseDto
        {
            Token = token,
            Usuario = new UserBasicInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Nombre = user.Nombre,
                RutaAvatar = user.RutaAvatar,
                Rol = user.Rol.Nombre
            }
        };

        return (true, response, null);
    }

    private string GenerateJwtToken(Models.Usuario user)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var secret = jwtSettings.GetValue<string>("Secret") ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var issuer = jwtSettings.GetValue<string>("Issuer");
        var audience = jwtSettings.GetValue<string>("Audience");
        var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes", 120);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Rol.Nombre)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
