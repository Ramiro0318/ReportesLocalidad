using System.Text.Json;
using ReportesLocalidadApp.Models.DTOs;

namespace ReportesLocalidadApp.Services;

public class ReportePendienteService
{
    private string rutaArchivo = Path.Combine(FileSystem.AppDataDirectory, "reportes_pendientes.json");

    public async Task GuardarReportePendienteAsync(SubirReporteDto reporte)
    {
        var acciones = await ObtenerAccionesPendientesAsync();

        if (string.IsNullOrWhiteSpace(reporte.ClientRequestId))
        {
            reporte.ClientRequestId = Guid.NewGuid().ToString();
        }

        var existe = acciones.Any(x => x.Tipo == "Crear" && x.SubirReporte?.ClientRequestId == reporte.ClientRequestId);

        if (!existe)
        {
            acciones.Add(new AccionPendienteDto
            {
                IdPendiente = reporte.ClientRequestId,
                Tipo = "Crear",
                SubirReporte = reporte
            });

            await GuardarListaAsync(acciones);
        }
    }

    public async Task GuardarEdicionPendienteAsync(int idReporte, EditarReporteDto reporte)
    {
        var acciones = await ObtenerAccionesPendientesAsync();
        acciones.RemoveAll(x => x.Tipo == "Editar" && x.IdReporte == idReporte);

        acciones.Add(new AccionPendienteDto
        {
            IdPendiente = Guid.NewGuid().ToString(),
            Tipo = "Editar",
            IdReporte = idReporte,
            EditarReporte = reporte
        });

        await GuardarListaAsync(acciones);
    }

    public async Task GuardarEliminacionPendienteAsync(int idReporte, int idUsuario)
    {
        var acciones = await ObtenerAccionesPendientesAsync();
        acciones.RemoveAll(x => x.IdReporte == idReporte);

        acciones.Add(new AccionPendienteDto
        {
            IdPendiente = Guid.NewGuid().ToString(),
            Tipo = "Eliminar",
            IdReporte = idReporte,
            IdUsuario = idUsuario
        });

        await GuardarListaAsync(acciones);
    }

    public async Task GuardarCambioEstadoPendienteAsync(int idReporte, CambiarEstadoReporteDto cambiarEstado)
    {
        var acciones = await ObtenerAccionesPendientesAsync();
        acciones.RemoveAll(x => x.Tipo == "CambiarEstado" && x.IdReporte == idReporte);

        acciones.Add(new AccionPendienteDto
        {
            IdPendiente = Guid.NewGuid().ToString(),
            Tipo = "CambiarEstado",
            IdReporte = idReporte,
            CambiarEstado = cambiarEstado
        });

        await GuardarListaAsync(acciones);
    }

    public async Task<List<AccionPendienteDto>> ObtenerAccionesPendientesAsync()
    {
        try
        {
            if (!File.Exists(rutaArchivo))
            {
                return new List<AccionPendienteDto>();
            }

            var json = await File.ReadAllTextAsync(rutaArchivo);
            var acciones = JsonSerializer.Deserialize<List<AccionPendienteDto>>(json);

            if (acciones != null && acciones.Any(x => !string.IsNullOrWhiteSpace(x.Tipo)))
            {
                return acciones;
            }

            var reportesAnteriores = JsonSerializer.Deserialize<List<SubirReporteDto>>(json);

            if (reportesAnteriores is null)
            {
                return new List<AccionPendienteDto>();
            }

            return reportesAnteriores.Select(reporte => new AccionPendienteDto
            {
                IdPendiente = reporte.ClientRequestId ?? Guid.NewGuid().ToString(),
                Tipo = "Crear",
                SubirReporte = reporte
            }).ToList();
        }
        catch
        {
            return new List<AccionPendienteDto>();
        }
    }

    public async Task EliminarReportePendienteAsync(string? clientRequestId)
    {
        await EliminarAccionPendienteAsync(clientRequestId);
    }

    public async Task EliminarAccionPendienteAsync(string? idPendiente)
    {
        if (string.IsNullOrWhiteSpace(idPendiente))
        {
            return;
        }

        var acciones = await ObtenerAccionesPendientesAsync();
        var accion = acciones.FirstOrDefault(x => x.IdPendiente == idPendiente || x.SubirReporte?.ClientRequestId == idPendiente);

        if (accion != null)
        {
            acciones.Remove(accion);
            await GuardarListaAsync(acciones);
        }
    }

    private async Task GuardarListaAsync(List<AccionPendienteDto> acciones)
    {
        var json = JsonSerializer.Serialize(acciones);
        await File.WriteAllTextAsync(rutaArchivo, json);
    }
}

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
