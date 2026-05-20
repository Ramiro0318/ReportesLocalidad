using System;
using System.Collections.Generic;

namespace ReportesLocalidadApi.Models.Entities;

public partial class Usuarios
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdRol { get; set; }

    public virtual Roles IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Reportes> Reportes { get; set; } = new List<Reportes>();
}
