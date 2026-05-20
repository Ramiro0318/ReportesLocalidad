namespace ReportesLocalidadApi.Models.DTOs;

public class ReportePropioDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; }
    public DateTime? FechaEdicion { get; set; }
    public int IdEstado { get; set; }
    public int IdCategoria { get; set; }
}

public class ReporteGeneralDto : ReportePropioDto 
{
    public string nombreUsuario { get; set; } = null!;
}
