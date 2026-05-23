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
}
