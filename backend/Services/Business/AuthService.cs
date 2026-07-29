using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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

    public async Task<(bool Success, LoginResponseDto? Data, string? Error)> RegisterAsync(RegisterRequestDto dto)
    {
        var usernameTrim = dto.Username.Trim().ToLower();

        bool usernameExists = await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == usernameTrim);
        if (usernameExists)
        {
            return (false, null, "El nombre de usuario ya se encuentra registrado.");
        }

        // Determinar el Rol (2: Coach, 3: Alumno)
        int idRol = dto.Rol.Trim().Equals("Coach", StringComparison.OrdinalIgnoreCase) ? 2 : 3;

        var rolObj = await _context.Roles.FirstOrDefaultAsync(r => r.Id == idRol);
        if (rolObj == null)
        {
            return (false, null, "El rol especificado no es válido.");
        }

        var newUser = new Usuario
        {
            Username = dto.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Nombre = dto.Nombre.Trim(),
            IdRol = idRol
        };

        if (idRol == 2 && dto.IdActividad.HasValue)
        {
            newUser.IdActividad = dto.IdActividad.Value;
        }

        _context.Usuarios.Add(newUser);
        await _context.SaveChangesAsync();

        // Si es Alumno, vincular o crear automáticamente su registro de Socio
        if (idRol == 3)
        {
            var planInicial = await _context.Planes.FirstOrDefaultAsync(p => p.Estado == "Activo")
                              ?? await _context.Planes.FirstAsync();

            var dni = string.IsNullOrWhiteSpace(dto.Dni) ? $"AUTO-{newUser.Id:D6}" : dto.Dni.Trim();

            var socio = new Socio
            {
                Dni = dni,
                NombreCompleto = newUser.Nombre,
                Telefono = dto.Telefono?.Trim(),
                Email = dto.Email?.Trim(),
                FechaAlta = DateTime.UtcNow,
                Estado = "Activo",
                IdPlan = dto.IdPlan.HasValue ? dto.IdPlan.Value : planInicial.Id,
                IdUsuario = newUser.Id,
                IdCoach = dto.IdCoach
            };

            _context.Socios.Add(socio);
            await _context.SaveChangesAsync();
        }

        // Generar JWT Token de inicio de sesión automático
        newUser.Rol = rolObj;
        var token = GenerateJwtToken(newUser);

        var response = new LoginResponseDto
        {
            Token = token,
            Usuario = new UserBasicInfoDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Nombre = newUser.Nombre,
                RutaAvatar = newUser.RutaAvatar,
                Rol = rolObj.Nombre
            }
        };

        return (true, response, null);
    }

    public async Task<(bool Success, UserBasicInfoDto? Data, string? Error)> GetUserProfileAsync(int userId)
    {
        var user = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.EliminadoAt.HasValue)
        {
            return (false, null, "Usuario no encontrado o deshabilitado.");
        }

        var dto = new UserBasicInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            Nombre = user.Nombre,
            RutaAvatar = user.RutaAvatar,
            Rol = user.Rol.Nombre
        };

        return (true, dto, null);
    }

    public async Task<(bool Success, UserBasicInfoDto? Data, string? Error)> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.EliminadoAt.HasValue)
        {
            return (false, null, "Usuario no encontrado o deshabilitado.");
        }

        user.Nombre = dto.Nombre.Trim();
        await _context.SaveChangesAsync();

        var result = new UserBasicInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            Nombre = user.Nombre,
            RutaAvatar = user.RutaAvatar,
            Rol = user.Rol.Nombre
        };

        return (true, result, null);
    }

    public async Task<(bool Success, string? RutaAvatar, string? Error)> UploadAvatarAsync(int userId, IFormFile file, string webRootPath)
    {
        if (file == null || file.Length == 0)
        {
            return (false, null, "Debe seleccionar un archivo válido.");
        }

        const long maxSizeBytes = 5 * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            return (false, null, "El tamaño máximo permitido para la imagen es 5 MB.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            return (false, null, "Formato de archivo no permitido. Formatos aceptados: JPG, JPEG, PNG, WEBP.");
        }

        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || user.EliminadoAt.HasValue)
        {
            return (false, null, "Usuario no encontrado o deshabilitado.");
        }

        var uploadsFolder = Path.Combine(webRootPath, "uploads", "avatares");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(destinationPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (!string.IsNullOrEmpty(user.RutaAvatar) && user.RutaAvatar.StartsWith("/uploads/avatares/"))
        {
            var oldFileName = Path.GetFileName(user.RutaAvatar);
            var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
            if (File.Exists(oldFilePath))
            {
                try
                {
                    File.Delete(oldFilePath);
                }
                catch
                {
                    // Ignore failure
                }
            }
        }

        user.RutaAvatar = $"/uploads/avatares/{uniqueFileName}";
        await _context.SaveChangesAsync();

        return (true, user.RutaAvatar, null);
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
