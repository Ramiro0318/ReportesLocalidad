using System.Net.Http.Json;
using ReportesLocalidadApp.Models.DTOs;

namespace ReportesLocalidadApp.Services;

public class ReportesApiService
{
    private readonly HttpClient http;
    private const string Endpoint = "api/reportes";

    public ReportesApiService(HttpClient http)
    {
        this.http = http;
    }

    public async Task<ApiResponse<ReporteDetalleDto>?> CrearReporteAsync(SubirReporteDto subirReporteDto)
    {
        try
        {
            var response = await http.PostAsJsonAsync($"{Endpoint}/crearReporte", subirReporteDto);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReporteDetalleDto>>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<ReporteDetalleDto>?> GetReporteAsync(int idReporte)
    {
        try
        {
            var result = await http.GetFromJsonAsync<ApiResponse<ReporteDetalleDto>>($"{Endpoint}/getReporte/{idReporte}");
            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<ReporteAEditarDto>?> GetReporteEditarAsync(int idReporte)
    {
        try
        {
            var result = await http.GetFromJsonAsync<ApiResponse<ReporteAEditarDto>>($"{Endpoint}/getReporteEditar/{idReporte}");
            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<ReporteDetalleDto>?> EditarReporteAsync(int idReporte, EditarReporteDto editarReporteDto)
    {
        try
        {
            var response = await http.PutAsJsonAsync($"{Endpoint}/editarReporte/{idReporte}", editarReporteDto);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReporteDetalleDto>>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<ReporteDetalleDto>?> CambiarEstadoAsync(int idReporte, CambiarEstadoReporteDto cambiarEstadoReporteDto)
    {
        try
        {
            var response = await http.PutAsJsonAsync($"{Endpoint}/cambiarEstado/{idReporte}", cambiarEstadoReporteDto);
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReporteDetalleDto>>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<List<ReportePropioDto>>?> GetByUsuarioAsync(int idUsuario, int cantidad = 25)
    {
        try
        {
            var result = await http.GetFromJsonAsync<ApiResponse<List<ReportePropioDto>>>($"{Endpoint}/getByUsuario/{idUsuario}?cantidad={cantidad}");

            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<List<ReporteGeneralDto>>?> GetReportesAsync(int skip = 0, int take = 50)
    {
        try
        {
            var result = await http.GetFromJsonAsync<ApiResponse<List<ReporteGeneralDto>>>($"{Endpoint}/getReportes?skip={skip}&take={take}");

            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<ReporteDetalleDto>?> EliminarReporteAsync(int idReporte, int idUsuario)
    {
        try
        {
            var response = await http.DeleteAsync($"{Endpoint}/eliminarReporte/{idReporte}?idUsuario={idUsuario}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<ReporteDetalleDto>>();
        }
        catch
        {
            return null;
        }
    }
}
