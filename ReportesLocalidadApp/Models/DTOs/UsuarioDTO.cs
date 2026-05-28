using System;
using System.Collections.Generic;
using System.Text;

namespace ReportesLocalidadApp.Models.DTOs
{

    public class RegistroDto
    {
        public string NombreUsuario { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int IdRol { get; set; } = 1;
    }

    public class LoginDto
    {
        public string NombreUsuario { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UsuarioRespuestaDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public int IdRol { get; set; }
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public UsuarioRespuestaDto Usuario { get; set; } = new();
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = "";
    }

    public class LogoutDto
    {
        public string RefreshToken { get; set; } = "";
    }

    public class TokenFirebaseDto
    {
        public string Token { get; set; } = "";
    }
}
