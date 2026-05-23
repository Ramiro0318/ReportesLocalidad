using System;
using System.Collections.Generic;
using System.Text;

namespace ReportesLocalidadApp.Models.DTOs
{
    public enum Categorias { Bache = 1, Fuga_de_agua = 2, Basura = 3, Alumbrado_Publico = 4, Accidente = 5, Otro = 6 }
    public enum Estados { Pendiente = 1, En_Progreso = 2, Resuelto = 3 }

    public class SubirReporteDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Foto { get; set; }
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
        public string? ClientRequestId { get; set; }
    }

    public class EditarReporteDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Foto { get; set; }
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
    }

    public class ReporteDetalleDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? ImgUrl { get; set; }
        public DateTime FechaSubida { get; set; }
        public DateTime? FechaEdicion { get; set; }
        public string? ClientRequestId { get; set; }
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
        public int IdEstado { get; set; }
    }

    public class ReporteAEditarDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? ImgUrl { get; set; }
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
    }

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
        public string NombreUsuario { get; set; } = string.Empty;
    }

    public class CambiarEstadoReporteDto
    {
        public int IdEstado { get; set; }
        public int IdUsuario { get; set; }
    }

}
