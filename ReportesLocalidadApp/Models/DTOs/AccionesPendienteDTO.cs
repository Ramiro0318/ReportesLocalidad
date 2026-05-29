using System;
using System.Collections.Generic;
using System.Text;

namespace ReportesLocalidadApp.Models.DTOs
{
    public class AccionPendienteDto
    {
        public string IdPendiente { get; set; } = "";
        public string Tipo { get; set; } = "";
        public int IdReporte { get; set; }
        public int IdUsuario { get; set; }
        public SubirReporteDto? SubirReporte { get; set; }
        public EditarReporteDto? EditarReporte { get; set; }
        public CambiarEstadoReporteDto? CambiarEstado { get; set; }
    }
}
