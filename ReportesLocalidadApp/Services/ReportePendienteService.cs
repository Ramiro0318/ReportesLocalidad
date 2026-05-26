using System.Text.Json;
using ReportesLocalidadApp.Models.DTOs;

namespace ReportesLocalidadApp.Services;

public class ReportePendienteService
{
    private readonly string rutaArchivo = Path.Combine(FileSystem.AppDataDirectory, "reportes_pendientes.json");

    public async Task GuardarReportePendienteAsync(SubirReporteDto reporte)
    {
        var reportes = await ObtenerReportesPendientesAsync();

        if (string.IsNullOrWhiteSpace(reporte.ClientRequestId))
        {
            reporte.ClientRequestId = Guid.NewGuid().ToString();
        }

        var existe = reportes.Any(x => x.ClientRequestId == reporte.ClientRequestId);

        if (!existe)
        {
            reportes.Add(reporte);
            await GuardarListaAsync(reportes);
        }
    }

    public async Task<List<SubirReporteDto>> ObtenerReportesPendientesAsync()
    {
        try
        {
            if (!File.Exists(rutaArchivo))
            {
                return new List<SubirReporteDto>();
            }

            var json = await File.ReadAllTextAsync(rutaArchivo);
            var reportes = JsonSerializer.Deserialize<List<SubirReporteDto>>(json);

            return reportes ?? new List<SubirReporteDto>();
        }
        catch
        {
            return new List<SubirReporteDto>();
        }
    }

    public async Task EliminarReportePendienteAsync(string? clientRequestId)
    {
        if (string.IsNullOrWhiteSpace(clientRequestId))
        {
            return;
        }

        var reportes = await ObtenerReportesPendientesAsync();
        var reporte = reportes.FirstOrDefault(x => x.ClientRequestId == clientRequestId);

        if (reporte is not null)
        {
            reportes.Remove(reporte);
            await GuardarListaAsync(reportes);
        }
    }

    private async Task GuardarListaAsync(List<SubirReporteDto> reportes)
    {
        var json = JsonSerializer.Serialize(reportes);
        await File.WriteAllTextAsync(rutaArchivo, json);
    }
}
