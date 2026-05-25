using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportesLocalidadApp.Models.DTOs;
using ReportesLocalidadApp.Services;

namespace ReportesLocalidadApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ReportesService reportesService;
    private readonly AuthService authService;
    private readonly List<ReporteGeneralDto> reportesCargados = new();
    private readonly List<ReportePropioDto> reportesPropiosCargados = new();
    private int reportesSaltados;
    private int reportesPropiosSaltados;
    private int elementosMostrados;
    private int elementosPropiosMostrados;
    private const int CargaInicial = 50;
    private const int CantidadVisible = 25;

    public MainViewModel(ReportesService reportesService, AuthService authService)
    {
        this.reportesService = reportesService;
        this.authService = authService;
        Reportes = new ObservableCollection<ReporteGeneralDto>();
        MisReportes = new ObservableCollection<ReportePropioDto>();
    }

    public ObservableCollection<ReporteGeneralDto> Reportes { get; }
    public ObservableCollection<ReportePropioDto> MisReportes { get; }
    public List<Categorias> CategoriasDisponibles { get; } = Enum.GetValues<Categorias>().ToList();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string mensaje = string.Empty;

    [ObservableProperty]
    private string tituloReporte = string.Empty;

    [ObservableProperty]
    private string descripcionReporte = string.Empty;

    [ObservableProperty]
    private Categorias categoriaSeleccionada = Categorias.Bache;

    [ObservableProperty]
    private ReporteDetalleDto? reporteDetalle;

    [ObservableProperty]
    private bool puedeEditarReporte;

    [ObservableProperty]
    private bool estaEditandoReporte;

    [ObservableProperty]
    private bool camposSoloLectura = true;

    [ObservableProperty]
    private string tituloDetalle = string.Empty;

    [ObservableProperty]
    private string descripcionDetalle = string.Empty;

    [ObservableProperty]
    private Categorias categoriaDetalle = Categorias.Bache;

    [RelayCommand]
    private async Task CargarReportes()
    {
        await CargarReportesGeneralesAsync();
    }

    [RelayCommand]
    private async Task CargarReportesAdmin()
    {
        await CargarReportesGeneralesAsync();
    }

    private async Task CargarReportesGeneralesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            reportesCargados.Clear();
            Reportes.Clear();
            reportesSaltados = 0;
            elementosMostrados = 0;

            var respuesta = await reportesService.GetReportesAsync(reportesSaltados, CargaInicial);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            reportesCargados.AddRange(respuesta.Data);
            reportesSaltados += respuesta.Data.Count;
            MostrarSiguientesReportes();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CargarMasReportesAdmin()
    {
        if (IsBusy)
        {
            return;
        }

        if (elementosMostrados < reportesCargados.Count)
        {
            MostrarSiguientesReportes();
            return;
        }

        try
        {
            IsBusy = true;

            var respuesta = await reportesService.GetReportesAsync(reportesSaltados, CantidadVisible);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            reportesCargados.AddRange(respuesta.Data);
            reportesSaltados += respuesta.Data.Count;
            MostrarSiguientesReportes();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void MostrarSiguientesReportes()
    {
        var reportesParaMostrar = reportesCargados
            .Skip(elementosMostrados)
            .Take(CantidadVisible)
            .ToList();

        foreach (var reporte in reportesParaMostrar)
        {
            Reportes.Add(reporte);
        }

        elementosMostrados += reportesParaMostrar.Count;
    }

    [RelayCommand]
    private async Task CargarMisReportes()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            reportesPropiosCargados.Clear();
            MisReportes.Clear();
            reportesPropiosSaltados = 0;
            elementosPropiosMostrados = 0;

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontro la sesion del usuario.";
                return;
            }

            var respuesta = await reportesService.GetByUsuarioAsync(idUsuario, reportesPropiosSaltados, CargaInicial);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            reportesPropiosCargados.AddRange(respuesta.Data);
            reportesPropiosSaltados += respuesta.Data.Count;
            MostrarSiguientesReportesPropios();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CargarMasMisReportes()
    {
        if (IsBusy)
        {
            return;
        }

        if (elementosPropiosMostrados < reportesPropiosCargados.Count)
        {
            MostrarSiguientesReportesPropios();
            return;
        }

        try
        {
            IsBusy = true;

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontro la sesion del usuario.";
                return;
            }

            var respuesta = await reportesService.GetByUsuarioAsync(idUsuario, reportesPropiosSaltados, CantidadVisible);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            reportesPropiosCargados.AddRange(respuesta.Data);
            reportesPropiosSaltados += respuesta.Data.Count;
            MostrarSiguientesReportesPropios();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void MostrarSiguientesReportesPropios()
    {
        var reportesParaMostrar = reportesPropiosCargados
            .Skip(elementosPropiosMostrados)
            .Take(CantidadVisible)
            .ToList();

        foreach (var reporte in reportesParaMostrar)
        {
            MisReportes.Add(reporte);
        }

        elementosPropiosMostrados += reportesParaMostrar.Count;
    }

    [RelayCommand]
    private async Task CrearReporte()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TituloReporte))
        {
            Mensaje = "Ingrese un titulo para el reporte.";
            return;
        }

        if (string.IsNullOrWhiteSpace(DescripcionReporte))
        {
            Mensaje = "Ingrese una descripcion para el reporte.";
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontro la sesion del usuario.";
                return;
            }

            var reporte = new SubirReporteDto
            {
                Titulo = TituloReporte.Trim(),
                Descripcion = DescripcionReporte.Trim(),
                Direccion = null,
                Foto = null,
                IdUsuario = idUsuario,
                IdCategoria = (int)CategoriaSeleccionada,
                ClientRequestId = Guid.NewGuid().ToString()
            };

            var respuesta = await reportesService.CrearReporteAsync(reporte);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            await AgregarReporteNuevoAListasAsync(respuesta.Data);

            TituloReporte = string.Empty;
            DescripcionReporte = string.Empty;
            CategoriaSeleccionada = Categorias.Bache;
            Mensaje = respuesta.Message;

            await Shell.Current.GoToAsync("reportes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AgregarReporteNuevoAListasAsync(ReporteDetalleDto reporte)
    {
        var nombreUsuario = await authService.GetNombreUsuarioAsync() ?? string.Empty;

        var reporteGeneral = new ReporteGeneralDto
        {
            Id = reporte.Id,
            Titulo = reporte.Titulo,
            FechaSubida = reporte.FechaSubida,
            FechaEdicion = reporte.FechaEdicion,
            IdEstado = reporte.IdEstado,
            IdCategoria = reporte.IdCategoria,
            NombreUsuario = nombreUsuario
        };

        var reportePropio = new ReportePropioDto
        {
            Id = reporte.Id,
            Titulo = reporte.Titulo,
            FechaSubida = reporte.FechaSubida,
            FechaEdicion = reporte.FechaEdicion,
            IdEstado = reporte.IdEstado,
            IdCategoria = reporte.IdCategoria
        };

        if (reportesCargados.Count > 0 || Reportes.Count > 0)
        {
            reportesCargados.Insert(0, reporteGeneral);
            Reportes.Insert(0, reporteGeneral);
            elementosMostrados++;
            reportesSaltados++;
        }

        if (reportesPropiosCargados.Count > 0 || MisReportes.Count > 0)
        {
            reportesPropiosCargados.Insert(0, reportePropio);
            MisReportes.Insert(0, reportePropio);
            elementosPropiosMostrados++;
            reportesPropiosSaltados++;
        }
    }

    [RelayCommand]
    private async Task AbrirReporte(ReportePropioDto reporte)
    {
        await CargarDetalleReporteAsync(reporte.Id, "reporte", false, true);
    }

    [RelayCommand]
    private async Task AbrirMiReporte(ReportePropioDto reporte)
    {
        await CargarDetalleReporteAsync(reporte.Id, "reporte", true);
    }

    [RelayCommand]
    private async Task AbrirReporteAdmin(ReporteGeneralDto reporte)
    {
        await CargarDetalleReporteAsync(reporte.Id, "reporteAdmin", false);
    }

    private async Task CargarDetalleReporteAsync(int idReporte, string ruta, bool puedeEditar, bool verificarPropietario = false)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            var respuesta = await reportesService.GetReporteAsync(idReporte);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            if (verificarPropietario)
            {
                var idUsuario = await authService.GetIdUsuarioAsync();
                puedeEditar = respuesta.Data.IdUsuario == idUsuario;
            }

            PrepararDetalleReporte(respuesta.Data, puedeEditar);
            await Shell.Current.GoToAsync(ruta);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PrepararDetalleReporte(ReporteDetalleDto reporte, bool puedeEditar)
    {
        ReporteDetalle = reporte;
        PuedeEditarReporte = puedeEditar;
        EstaEditandoReporte = false;
        CamposSoloLectura = true;
        TituloDetalle = reporte.Titulo;
        DescripcionDetalle = reporte.Descripcion;
        CategoriaDetalle = (Categorias)reporte.IdCategoria;
    }

    [RelayCommand]
    private void ActivarEdicionReporte()
    {
        if (!PuedeEditarReporte)
        {
            return;
        }

        EstaEditandoReporte = true;
        CamposSoloLectura = false;
        Mensaje = string.Empty;
    }

    [RelayCommand]
    private async Task GuardarEdicionReporte()
    {
        if (IsBusy || ReporteDetalle is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TituloDetalle))
        {
            Mensaje = "Ingrese un titulo para el reporte.";
            return;
        }

        if (string.IsNullOrWhiteSpace(DescripcionDetalle))
        {
            Mensaje = "Ingrese una descripcion para el reporte.";
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontro la sesion del usuario.";
                return;
            }

            var reporteEditado = new EditarReporteDto
            {
                Id = ReporteDetalle.Id,
                Titulo = TituloDetalle.Trim(),
                Descripcion = DescripcionDetalle.Trim(),
                Direccion = ReporteDetalle.Direccion,
                Foto = null,
                IdUsuario = idUsuario,
                IdCategoria = (int)CategoriaDetalle
            };

            var respuesta = await reportesService.EditarReporteAsync(ReporteDetalle.Id, reporteEditado);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            PrepararDetalleReporte(respuesta.Data, true);
            ActualizarReporteEnListas(respuesta.Data);
            Mensaje = respuesta.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EliminarReporte()
    {
        if (IsBusy || ReporteDetalle is null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlert("Eliminar reporte", "Desea eliminar este reporte?", "Si", "No");

        if (!confirmar)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontro la sesion del usuario.";
                return;
            }

            var respuesta = await reportesService.EliminarReporteAsync(ReporteDetalle.Id, idUsuario);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success)
            {
                Mensaje = respuesta.Message;
                return;
            }

            QuitarReporteDeListas(ReporteDetalle.Id);
            ReporteDetalle = null;
            Mensaje = respuesta.Message;
            await Shell.Current.GoToAsync("misReportes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ActualizarReporteEnListas(ReporteDetalleDto reporte)
    {
        var reportePropioActualizado = new ReportePropioDto
        {
            Id = reporte.Id,
            Titulo = reporte.Titulo,
            FechaSubida = reporte.FechaSubida,
            FechaEdicion = reporte.FechaEdicion,
            IdEstado = reporte.IdEstado,
            IdCategoria = reporte.IdCategoria
        };

        var reportePropio = MisReportes.FirstOrDefault(x => x.Id == reporte.Id);

        if (reportePropio is not null)
        {
            var posicion = MisReportes.IndexOf(reportePropio);
            MisReportes[posicion] = reportePropioActualizado;
        }

        var reportePropioCargado = reportesPropiosCargados.FirstOrDefault(x => x.Id == reporte.Id);

        if (reportePropioCargado is not null)
        {
            var posicion = reportesPropiosCargados.IndexOf(reportePropioCargado);
            reportesPropiosCargados[posicion] = reportePropioActualizado;
        }

        var reporteGeneral = Reportes.FirstOrDefault(x => x.Id == reporte.Id);

        if (reporteGeneral is not null)
        {
            var reporteGeneralActualizado = new ReporteGeneralDto
            {
                Id = reporte.Id,
                Titulo = reporte.Titulo,
                FechaSubida = reporte.FechaSubida,
                FechaEdicion = reporte.FechaEdicion,
                IdEstado = reporte.IdEstado,
                IdCategoria = reporte.IdCategoria,
                NombreUsuario = reporteGeneral.NombreUsuario
            };

            var posicion = Reportes.IndexOf(reporteGeneral);
            Reportes[posicion] = reporteGeneralActualizado;

            var reporteGeneralCargado = reportesCargados.FirstOrDefault(x => x.Id == reporte.Id);

            if (reporteGeneralCargado is not null)
            {
                var posicionCargado = reportesCargados.IndexOf(reporteGeneralCargado);
                reportesCargados[posicionCargado] = reporteGeneralActualizado;
            }
        }
    }

    private void QuitarReporteDeListas(int idReporte)
    {
        var reportePropio = MisReportes.FirstOrDefault(x => x.Id == idReporte);

        if (reportePropio is not null)
        {
            MisReportes.Remove(reportePropio);
        }

        var reportePropioCargado = reportesPropiosCargados.FirstOrDefault(x => x.Id == idReporte);

        if (reportePropioCargado is not null)
        {
            reportesPropiosCargados.Remove(reportePropioCargado);
        }

        var reporteGeneral = Reportes.FirstOrDefault(x => x.Id == idReporte);

        if (reporteGeneral is not null)
        {
            Reportes.Remove(reporteGeneral);
        }

        var reporteGeneralCargado = reportesCargados.FirstOrDefault(x => x.Id == idReporte);

        if (reporteGeneralCargado is not null)
        {
            reportesCargados.Remove(reporteGeneralCargado);
        }
    }

    [RelayCommand]
    private async Task VolverReporte()
    {
        if (PuedeEditarReporte)
        {
            await Shell.Current.GoToAsync("misReportes");
            return;
        }

        await Shell.Current.GoToAsync("reportes");
    }

    [RelayCommand]
    private async Task IrInicio()
    {
        await Shell.Current.GoToAsync("reportes");
    }

    [RelayCommand]
    private async Task IrAgregarReporte()
    {
        await Shell.Current.GoToAsync("agregarReporte");
    }

    [RelayCommand]
    private async Task IrMisReportes()
    {
        await Shell.Current.GoToAsync("misReportes");
    }

    [RelayCommand]
    private async Task IrPerfil()
    {
        await Shell.Current.GoToAsync("perfil");
    }

    [RelayCommand]
    private async Task IrAdminReportes()
    {
        await Shell.Current.GoToAsync("adminReportes");
    }
}
