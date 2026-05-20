namespace ReportesLocalidadApi.Models.DTOs;

public class RegistroDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IdRol { get; set; } = 1;
}
