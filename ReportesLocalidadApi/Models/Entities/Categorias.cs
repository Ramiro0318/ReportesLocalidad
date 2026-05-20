using System;
using System.Collections.Generic;

namespace ReportesLocalidadApi.Models.Entities;

public partial class Categorias
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Reportes> Reportes { get; set; } = new List<Reportes>();
}
