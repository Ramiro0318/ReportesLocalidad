namespace ReportesLocalidadApi.Models.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UsuarioRespuestaDto Usuario { get; set; } = new();
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
