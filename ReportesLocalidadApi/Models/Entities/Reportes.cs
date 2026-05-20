using System;
using System.Collections.Generic;

namespace ReportesLocalidadApi.Models.Entities;

public partial class Reportes
{
    public int Id { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? Direccion { get; set; }

    public string? ImgUrl { get; set; }

    public DateTime FechaSubida { get; set; }

    public DateTime? FechaEdicion { get; set; }

    public string? ClientRequestId { get; set; }

    public int IdUsuario { get; set; }

    public int IdCategoria { get; set; }

    public int IdEstado { get; set; }

    public virtual Categorias IdCategoriaNavigation { get; set; } = null!;

    public virtual Estados IdEstadoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
