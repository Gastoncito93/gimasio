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
}
