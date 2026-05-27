using System.Net.Http.Headers;
using System.Net.Http.Json;
using ReportesLocalidadApp.Models.DTOs;

namespace ReportesLocalidadApp.Services;

public class AuthService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string IdUsuarioKey = "id_usuario";
    private const string NombreUsuarioKey = "nombre_usuario";
    private const string IdRolKey = "id_rol";

    private readonly HttpClient client;
    private readonly FirebaseTokenService firebaseTokenService;

    public AuthService(HttpClient client, FirebaseTokenService firebaseTokenService)
    {
        this.client = client;
        this.firebaseTokenService = firebaseTokenService;
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

            var response = await client.PostAsJsonAsync("api/auth/login", loginDto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
                return new ApiResponse<UsuarioRespuestaDto>
                {
                    Success = false,
                    Message = error?.Message ?? "No se pudo iniciar sesion."
                };
            }

            var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();

            if (resultado?.Success == true && resultado.Data is not null)
            {
                await GuardarSesionAsync(resultado.Data);

                return new ApiResponse<UsuarioRespuestaDto>
                {
                    Success = true,
                    Message = resultado.Message,
                    Data = resultado.Data.Usuario
                };
            }

            return new ApiResponse<UsuarioRespuestaDto>
            {
                Success = false,
                Message = resultado?.Message ?? "No se pudo iniciar sesion."
            };
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

            var response = await client.PostAsJsonAsync("api/auth/registro", registroDto);
            return await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioRespuestaDto>>();
        }
        catch
        {
            return null;
        }
    }

    public async Task GuardarSesionAsync(AuthResponseDto authResponse)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, authResponse.AccessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, authResponse.RefreshToken);
        await SecureStorage.Default.SetAsync(IdUsuarioKey, authResponse.Usuario.Id.ToString());
        await SecureStorage.Default.SetAsync(NombreUsuarioKey, authResponse.Usuario.NombreUsuario);
        await SecureStorage.Default.SetAsync(IdRolKey, authResponse.Usuario.IdRol.ToString());

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        await EnviarTokenFirebaseAsync();
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

    public async Task<UsuarioRespuestaDto?> ObtenerSesionGuardadaAsync()
    {
        var sesion = await ObtenerSesionAsync();

        if (sesion is null)
        {
            return null;
        }

        await PrepararTokenAsync();

        var tokenActualizado = await RefreshTokenAsync();

        if (!tokenActualizado)
        {
            return null;
        }

        return await ObtenerSesionAsync();
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

    public async Task<string?> GetAccessTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(AccessTokenKey);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(RefreshTokenKey);
    }

    public async Task PrepararTokenAsync()
    {
        var accessToken = await GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    public async Task EnviarTokenFirebaseAsync()
    {
        try
        {
            await PrepararTokenAsync();
            var tokenFirebase = await firebaseTokenService.ObtenerTokenAsync();

            if (string.IsNullOrWhiteSpace(tokenFirebase))
            {
                return;
            }

            var tokenFirebaseDto = new TokenFirebaseDto
            {
                Token = tokenFirebase
            };

            await client.PostAsJsonAsync("api/usuarios/guardarTokenFirebase", tokenFirebaseDto);
        }
        catch
        {
        }
    }

    public async Task<bool> EstaAutenticadoAsync()
    {
        var accessToken = await GetAccessTokenAsync();
        return !string.IsNullOrWhiteSpace(accessToken);
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await GetRefreshTokenAsync();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = refreshToken
            };

            var response = await client.PostAsJsonAsync("api/auth/refresh", refreshTokenDto);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var resultado = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();

            if (resultado?.Success == true && resultado.Data is not null)
            {
                await GuardarSesionAsync(resultado.Data);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task CerrarSesionAsync()
    {
        var refreshToken = await GetRefreshTokenAsync();

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            try
            {
                await PrepararTokenAsync();

                var logoutDto = new LogoutDto
                {
                    RefreshToken = refreshToken
                };

                await client.PostAsJsonAsync("api/usuarios/logout", logoutDto);
            }
            catch
            {
            }
        }

        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(IdUsuarioKey);
        SecureStorage.Default.Remove(NombreUsuarioKey);
        SecureStorage.Default.Remove(IdRolKey);
        client.DefaultRequestHeaders.Authorization = null;
    }
}
