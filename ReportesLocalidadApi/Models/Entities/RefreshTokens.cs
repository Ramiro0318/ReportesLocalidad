using System;
using System.Collections.Generic;

namespace ReportesLocalidadApi.Models.Entities;

public partial class RefreshTokens
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public DateTime? FechaRevocacion { get; set; }

    public int IdUsuario { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
