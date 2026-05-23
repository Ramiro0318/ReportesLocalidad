using ReportesLocalidadApp.Models.DTOs;

namespace ReportesLocalidadApp.Services;

public class SessionService
{
    private const string IdUsuarioKey = "";
    private const string NombreUsuarioKey = "";
    private const string IdRolKey = "";

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

        if (!int.TryParse(idTexto, out var idUsuario) || !int.TryParse(idRolTexto, out var idRol) || string.IsNullOrWhiteSpace(nombreUsuario))
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

    public async Task<bool> ExisteSesion()
    {
        var sesion = await ObtenerSesionAsync();
        return sesion is not null;
    }

    public void CerrarSesion()
    {
        SecureStorage.Default.Remove(IdUsuarioKey);
        SecureStorage.Default.Remove(NombreUsuarioKey);
        SecureStorage.Default.Remove(IdRolKey);
    }
}
