namespace Backend.DTOs.Auth;

public class UserBasicInfoDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? RutaAvatar { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool DebeCambiarPassword { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public bool DebeCambiarPassword { get; set; }
    public UserBasicInfoDto Usuario { get; set; } = new();
}
