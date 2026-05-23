using System.Net.Http.Json;
using ReportesLocalidadApp.Models.DTOs;

namespace ReportesLocalidadApp.Services;

public class AuthService
{
    private const string IdUsuarioKey = "id_usuario";
    private const string NombreUsuarioKey = "nombre_usuario";
    private const string IdRolKey = "id_rol";

    private readonly HttpClient client;

    public AuthService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<ApiResponse<UsuarioRespuestaDto>?> LoginAsync(string nombreUsuario, string password)
    {
        try
        {
            var loginDto = new LoginDto
            {
                NombreUsuario = nombreUsuario,
                Password = password
            };

            var response = await client.PostAsJsonAsync("api/usuarios/login", loginDto);

            if (!response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioRespuestaDto>>();
            }

            var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioRespuestaDto>>();

            if (resultado?.Success == true && resultado.Data is not null)
            {
                await GuardarSesionAsync(resultado.Data);
            }

            return resultado;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<UsuarioRespuestaDto>?> RegistrarAsync(string nombreUsuario, string password)
    {
        try
        {
            var registroDto = new RegistroDto
            {
                NombreUsuario = nombreUsuario,
                Password = password,
                IdRol = 1
            };

            var response = await client.PostAsJsonAsync("api/usuarios/registro", registroDto);
            return await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioRespuestaDto>>();
        }
        catch
        {
            return null;
        }
    }

    public async Task GuardarSesionAsync(UsuarioRespuestaDto usuario)
    {
        await SecureStorage.Default.SetAsync(IdUsuarioKey, usuario.Id.ToString());
        await SecureStorage.Default.SetAsync(NombreUsuarioKey, usuario.NombreUsuario);
        await SecureStorage.Default.SetAsync(IdRolKey, usuario.IdRol.ToString());
    }

    public async Task<UsuarioRespuestaDto?> ObtenerSesionAsync()
    {
        var idTexto = await SecureStorage.Default.GetAsync(IdUsuarioKey);
        var nombreUsuario = await SecureStorage.Default.GetAsync(NombreUsuarioKey);
        var idRolTexto = await SecureStorage.Default.GetAsync(IdRolKey);

        if (!int.TryParse(idTexto, out var idUsuario) ||
            !int.TryParse(idRolTexto, out var idRol) ||
            string.IsNullOrWhiteSpace(nombreUsuario))
        {
            return null;
        }

        return new UsuarioRespuestaDto
        {
            Id = idUsuario,
            NombreUsuario = nombreUsuario,
            IdRol = idRol
        };
    }

    public async Task<int> GetIdUsuarioAsync()
    {
        var idTexto = await SecureStorage.Default.GetAsync(IdUsuarioKey);
        return int.TryParse(idTexto, out var idUsuario) ? idUsuario : 0;
    }

    public async Task<string?> GetNombreUsuarioAsync()
    {
        return await SecureStorage.Default.GetAsync(NombreUsuarioKey);
    }

    public async Task<int> GetIdRolAsync()
    {
        var idRolTexto = await SecureStorage.Default.GetAsync(IdRolKey);
        return int.TryParse(idRolTexto, out var idRol) ? idRol : 0;
    }

    public async Task<bool> EstaAutenticadoAsync()
    {
        var idUsuario = await GetIdUsuarioAsync();
        return idUsuario != 0;
    }

    public void CerrarSesion()
    {
        SecureStorage.Default.Remove(IdUsuarioKey);
        SecureStorage.Default.Remove(NombreUsuarioKey);
        SecureStorage.Default.Remove(IdRolKey);
    }
}
