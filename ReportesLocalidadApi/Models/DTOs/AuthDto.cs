namespace ReportesLocalidadApi.Models.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public UsuarioRespuestaDto Usuario { get; set; } = new();
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = "";
}

public class LogoutDto
{
    public string RefreshToken { get; set; } = "";
}
