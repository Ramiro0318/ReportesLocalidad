namespace ReportesLocalidadApi.Models.DTOs;

public class UsuarioRespuestaDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public int IdRol { get; set; }
}
