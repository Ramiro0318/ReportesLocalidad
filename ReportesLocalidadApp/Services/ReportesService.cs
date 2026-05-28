using System.Net.Http.Json;
using System.Net;
using ReportesLocalidadApp.Models.DTOs;

namespace ReportesLocalidadApp.Services;

public class ReportesService
{
    private readonly HttpClient http;
    private readonly AuthService authService;
    private const string Endpoint = "api/reportes";
    public string UltimoErrorConexion { get; private set; } = string.Empty;

    public ReportesService(HttpClient http, AuthService authService)
    {
        this.http = http;
        this.authService = authService;
    }

    public string? ObtenerUrlImagen(string? imgUrl)
    {
        if (string.IsNullOrWhiteSpace(imgUrl)) { return null; }

        if (imgUrl.StartsWith("http")) { return imgUrl; }

        return new Uri(http.BaseAddress!, imgUrl.TrimStart('/')).ToString();
    }

    public async Task<bool> ApiDisponibleAsync()
    {
        try
        {
            var response = await http.GetAsync("api/hello");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ApiResponse<ReporteDetalleDto>?> CrearReporteAsync(SubirReporteDto subirReporteDto)
    {
        try
        {
            UltimoErrorConexion = string.Empty;
            await authService.PrepararTokenAsync();
            var response = await http.PostAsJsonAsync($"{Endpoint}/crearReporte", subirReporteDto);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<ReporteDetalleDto>();
                }

                response = await http.PostAsJsonAsync($"{Endpoint}/crearReporte", subirReporteDto);
            }

            return await LeerRespuestaAsync<ReporteDetalleDto>(response);
        }
        catch (TaskCanceledException ex)
        {
            UltimoErrorConexion = ex.Message;
            return null;
        }
        catch (HttpRequestException ex)
        {
            UltimoErrorConexion = ex.Message;
            return null;
        }
        catch
        {
            return new ApiResponse<ReporteDetalleDto>
            {
                Success = false,
                Message = "La API respondio, pero no se pudo leer la respuesta."
            };
        }
    }

    public async Task<ApiResponse<ReporteDetalleDto>?> GetReporteAsync(int idReporte)
    {
        try
        {
            await authService.PrepararTokenAsync();
            var response = await http.GetAsync($"{Endpoint}/getReporte/{idReporte}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<ReporteDetalleDto>();
                }

                response = await http.GetAsync($"{Endpoint}/getReporte/{idReporte}");
            }

            return await LeerRespuestaAsync<ReporteDetalleDto>(response);
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
            await authService.PrepararTokenAsync();
            var response = await http.GetAsync($"{Endpoint}/getReporteEditar/{idReporte}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<ReporteAEditarDto>();
                }

                response = await http.GetAsync($"{Endpoint}/getReporteEditar/{idReporte}");
            }

            return await LeerRespuestaAsync<ReporteAEditarDto>(response);
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
            await authService.PrepararTokenAsync();
            var response = await http.PutAsJsonAsync($"{Endpoint}/editarReporte/{idReporte}", editarReporteDto);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<ReporteDetalleDto>();
                }

                response = await http.PutAsJsonAsync($"{Endpoint}/editarReporte/{idReporte}", editarReporteDto);
            }

            return await LeerRespuestaAsync<ReporteDetalleDto>(response);
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
            await authService.PrepararTokenAsync();
            var response = await http.PutAsJsonAsync($"{Endpoint}/cambiarEstado/{idReporte}", cambiarEstadoReporteDto);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<ReporteDetalleDto>();
                }

                response = await http.PutAsJsonAsync($"{Endpoint}/cambiarEstado/{idReporte}", cambiarEstadoReporteDto);
            }

            return await LeerRespuestaAsync<ReporteDetalleDto>(response);
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<List<ReportePropioDto>>?> GetByUsuarioAsync(int idUsuario, int skip = 0, int take = 50)
    {
        try
        {
            await authService.PrepararTokenAsync();
            var response = await http.GetAsync($"{Endpoint}/getByUsuario/{idUsuario}?skip={skip}&take={take}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<List<ReportePropioDto>>();
                }

                response = await http.GetAsync($"{Endpoint}/getByUsuario/{idUsuario}?skip={skip}&take={take}");
            }

            return await LeerRespuestaAsync<List<ReportePropioDto>>(response);
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
            await authService.PrepararTokenAsync();
            var response = await http.GetAsync($"{Endpoint}/getReportes?skip={skip}&take={take}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<List<ReporteGeneralDto>>();
                }

                response = await http.GetAsync($"{Endpoint}/getReportes?skip={skip}&take={take}");
            }

            return await LeerRespuestaAsync<List<ReporteGeneralDto>>(response);
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
            await authService.PrepararTokenAsync();
            var response = await http.DeleteAsync($"{Endpoint}/eliminarReporte/{idReporte}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokenActualizado = await authService.RefreshTokenAsync();

                if (!tokenActualizado)
                {
                    return CrearRespuestaNoAutorizada<ReporteDetalleDto>();
                }

                response = await http.DeleteAsync($"{Endpoint}/eliminarReporte/{idReporte}");
            }

            return await LeerRespuestaAsync<ReporteDetalleDto>(response);
        }
        catch
        {
            return null;
        }
    }

    private async Task<ApiResponse<T>?> LeerRespuestaAsync<T>(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return CrearRespuestaNoAutorizada<T>();
        }

        if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = "La imagen es demasiado grande para el servidor."
            };
        }

        ApiResponse<T>? respuesta = null;

        try
        {
            respuesta = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }
        catch
        {
            respuesta = null;
        }

        if (respuesta != null)
        {
            return respuesta;
        }

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"La API rechazo la solicitud. Codigo: {(int)response.StatusCode}"
            };
        }

        return new ApiResponse<T>
        {
            Success = false,
            Message = "La API no devolvio una respuesta valida."
        };
    }

    private ApiResponse<T> CrearRespuestaNoAutorizada<T>()
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Sesion vencida. Inicia sesion nuevamente."
        };
    }
}
