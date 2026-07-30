namespace Backend.Services.Interfaces;

using Backend.DTOs.Auth;
using Microsoft.AspNetCore.Http;

public interface IAuthService
{
    Task<(bool Success, LoginResponseDto? Data, string? Error)> LoginAsync(LoginRequestDto dto);
    Task<(bool Success, LoginResponseDto? Data, string? Error)> RegisterAsync(RegisterRequestDto dto);
    Task<(bool Success, UserBasicInfoDto? Data, string? Error)> GetUserProfileAsync(int userId);
    Task<(bool Success, UserBasicInfoDto? Data, string? Error)> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<(bool Success, string? RutaAvatar, string? Error)> UploadAvatarAsync(int userId, IFormFile file, string webRootPath);
    Task<(bool Success, LoginResponseDto? Data, string? Error)> CambiarPasswordPrimerIngresoAsync(int userId, CambiarPasswordPrimerIngresoDto dto);
    Task<(bool Success, string? CodigoDev, string? Error)> SolicitarRecuperacionAsync(SolicitarRecuperacionDto dto);
    Task<(bool Success, string? Message, string? Error)> RestablecerPasswordConCodigoAsync(RestablecerPasswordConCodigoDto dto);
}
