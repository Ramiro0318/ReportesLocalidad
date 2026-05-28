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
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string? Direccion { get; set; }
        public string? Foto { get; set; }
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
        public string? ClientRequestId { get; set; }
    }

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

    public class ReporteDetalleDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string? Direccion { get; set; }
        public string? ImgUrl { get; set; }
        public DateTime FechaSubida { get; set; }
        public DateTime? FechaEdicion { get; set; }
        public string? ClientRequestId { get; set; }
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
        public int IdEstado { get; set; }
        public string CategoriaTexto
        {
            get
            {
                if (IdCategoria == (int)Categorias.Bache) return "Bache";
                if (IdCategoria == (int)Categorias.Fuga_de_agua) return "Fuga de agua";
                if (IdCategoria == (int)Categorias.Basura) return "Basura";
                if (IdCategoria == (int)Categorias.Alumbrado_Publico) return "Alumbrado publico";
                if (IdCategoria == (int)Categorias.Accidente) return "Accidente";
                if (IdCategoria == (int)Categorias.Otro) return "Otro";
                return "Sin categoria";
            }
        }
        public string EstadoTexto
        {
            get
            {
                if (IdEstado == (int)Estados.Pendiente)return "Pendiente";
                if (IdEstado == (int)Estados.En_Progreso) return "En progreso";
                if (IdEstado == (int)Estados.Resuelto) return "Resuelto";
                return "Sin estado";
            }
        }
    }

    public class ReporteAEditarDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string? Direccion { get; set; }
        public string? ImgUrl { get; set; }
        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }
    }

    public class ReportePropioDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public DateTime FechaSubida { get; set; }
        public DateTime? FechaEdicion { get; set; }
        public int IdEstado { get; set; }
        public int IdCategoria { get; set; }
        public string CategoriaTexto
        {
            get
            {
                if (IdCategoria == (int)Categorias.Bache) return "Bache";
                if (IdCategoria == (int)Categorias.Fuga_de_agua) return "Fuga de agua";
                if (IdCategoria == (int)Categorias.Basura) return "Basura";
                if (IdCategoria == (int)Categorias.Alumbrado_Publico) return "Alumbrado publico";
                if (IdCategoria == (int)Categorias.Accidente) return "Accidente";
                if (IdCategoria == (int)Categorias.Otro) return "Otro";
                return "Sin categoria";
            }
        }
        public string EstadoTexto
        {
            get
            {
                if (IdEstado == (int)Estados.Pendiente)return "Pendiente";
                if (IdEstado == (int)Estados.En_Progreso)return "En progreso";
                if (IdEstado == (int)Estados.Resuelto)return "Resuelto";
                return "Sin estado";
            }
        }
    }

    public class ReporteGeneralDto : ReportePropioDto
    {
        public string NombreUsuario { get; set; } = "";
    }

    public class CambiarEstadoReporteDto
    {
        public int IdEstado { get; set; }
        public int IdUsuario { get; set; }
    }

}
