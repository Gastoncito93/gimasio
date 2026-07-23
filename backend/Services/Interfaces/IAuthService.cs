namespace Backend.Services.Interfaces;

using Backend.DTOs.Auth;

public interface IAuthService
{
    Task<(bool Success, LoginResponseDto? Data, string? Error)> LoginAsync(LoginRequestDto dto);
}
