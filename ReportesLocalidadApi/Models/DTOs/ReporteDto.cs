
namespace ReportesLocalidadApi.Models.DTOs;


public class SubirReporteDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Foto { get; set; }
    public int IdUsuario { get; set; }
    public int IdCategoria { get; set; }
    public string? ClientRequestId { get; set; }
}

//public class ReporteAEditarDto
//{
//    public int Id { get; set; }
//    public string Titulo { get; set; } = "";
//    public string Descripcion { get; set; } = "";
//    public string? Direccion { get; set; }
//    public string? ImgUrl { get; set; }
//    public int IdUsuario { get; set; }
//    public int IdCategoria { get; set; }
//}

public class EditarReporteDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Foto { get; set; }
    public int IdUsuario { get; set; }
    public int IdCategoria { get; set; }
}

public class CambiarEstadoReporteDto
{
    public int IdEstado { get; set; }
    public int IdUsuario { get; set; }
}
