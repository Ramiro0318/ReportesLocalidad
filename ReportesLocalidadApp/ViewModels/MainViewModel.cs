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
    private readonly FotoService fotoService;
    private readonly ReportePendienteService reportePendienteService;
    private readonly AlertaService alertaService;
    private readonly List<ReporteGeneralDto> reportesCargados = new();
    private readonly List<ReportePropioDto> reportesPropiosCargados = new();

    private string? fotoBase64;
    private bool enviandoReportesPendientes;
    private bool procesandoFoto;
    private bool estaNavegando;
    private bool cargandoReportesGenerales;
    private bool cargandoReportesPropios;
    private bool cargandoPerfil;
    private string estadoOriginalReporte = "Pendiente";
    private bool preparandoDetalleReporte;
    private bool estadoReporteFueModificado;
    private int reportesSaltados;
    private int reportesPropiosSaltados;
    private int elementosMostrados;
    private int elementosPropiosMostrados;
    private int CargaInicial = 50;
    private int CantidadVisible = 25;

    public MainViewModel(ReportesService reportesService, AuthService authService, FotoService fotoService, ReportePendienteService reportePendienteService, AlertaService alertaService)
    {
        this.reportesService = reportesService;
        this.authService = authService;
        this.fotoService = fotoService;
        this.reportePendienteService = reportePendienteService;
        this.alertaService = alertaService;
        Reportes = new ObservableCollection<ReporteGeneralDto>();
        MisReportes = new ObservableCollection<ReportePropioDto>();
        Connectivity.Current.ConnectivityChanged += async (sender, args) => await RevisarConexionAsync();
    }

    public ObservableCollection<ReporteGeneralDto> Reportes { get; }
    public ObservableCollection<ReportePropioDto> MisReportes { get; }
    public List<string> CategoriasDisponibles { get; } = new()
    {
        "Bache",
        "Fuga de agua",
        "Basura",
        "Alumbrado publico",
        "Accidente",
        "Otro"
    };

    public List<string> EstadosDisponibles { get; } = new()
    {
        "Pendiente",
        "En progreso",
        "Resuelto"
    };

    public List<string> CategoriasFiltro { get; } = new()
    {
        "Todos",
        "Bache",
        "Fuga de agua",
        "Basura",
        "Alumbrado publico",
        "Accidente",
        "Otro"
    };

    public List<string> EstadosFiltro { get; } = new()
    {
        "Todos",
        "Pendiente",
        "En progreso",
        "Resuelto"
    };

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string mensaje = "";

    [ObservableProperty]
    private int reportesPendientes;

    [ObservableProperty]
    private int totalMisReportes;

    [ObservableProperty]
    private int reportesAtendidos;

    [ObservableProperty]
    private int reportesPendientesUsuario;

    [ObservableProperty]
    private string totalMisReportesTexto = "0";

    [ObservableProperty]
    private string reportesAtendidosTexto = "0";

    [ObservableProperty]
    private string reportesPendientesUsuarioTexto = "0";

    [ObservableProperty]
    private string nombreUsuario = "";

    [ObservableProperty]
    private string tituloReporte = "";

    [ObservableProperty]
    private string descripcionReporte = "";

    [ObservableProperty]
    private string categoriaSeleccionada = "Bache";

    [ObservableProperty]
    private string? rutaImagen;

    [ObservableProperty]
    private ImageSource? imagenSeleccionada;

    [ObservableProperty]
    private bool mostrarTextoImagen = true;

    [ObservableProperty]
    private ReporteDetalleDto? reporteDetalle;

    [ObservableProperty]
    private string? imagenDetalleUrl;

    [ObservableProperty]
    private bool puedeEditarReporte;

    [ObservableProperty]
    private bool estaEditandoReporte;

    [ObservableProperty]
    private bool camposSoloLectura = true;

    [ObservableProperty]
    private string tituloDetalle = "";

    [ObservableProperty]
    private string descripcionDetalle = "";

    [ObservableProperty]
    private string categoriaDetalle = "Bache";

    [ObservableProperty]
    private string estadoSeleccionado = "Pendiente";

    [ObservableProperty]
    private bool puedeConfirmarCambioEstado;

    partial void OnEstadoSeleccionadoChanged(string value)
    {
        if (preparandoDetalleReporte)
        {
            return;
        }

        if (value != estadoOriginalReporte || estadoReporteFueModificado)
        {
            estadoReporteFueModificado = true;
            PuedeConfirmarCambioEstado = true;
        }
    }

    [ObservableProperty]
    private string estadoFiltroSeleccionado = "Todos";

    [ObservableProperty]
    private string categoriaFiltroSeleccionada = "Todos";

    partial void OnEstadoFiltroSeleccionadoChanged(string value)    //Estos son métodos evento que detectan el cambio de una observablele Property
    {
        AplicarFiltrosReportesAdmin();
    }

    partial void OnCategoriaFiltroSeleccionadaChanged(string value)
    {
        AplicarFiltrosReportesAdmin();
    }

    [RelayCommand]
    private async Task CargarReportes()
    {
        try
        {
            await CargarReportesGeneralesAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task CargarReportesAdmin()
    {
        try
        {
            await CargarReportesGeneralesAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task CargarReportesGeneralesAsync()
    {
        if (cargandoReportesGenerales)
        {
            return;
        }

        try
        {
            cargandoReportesGenerales = true;
            IsBusy = true;
            Mensaje = "";
            await CargarNombreUsuarioAsync();

            var respuesta = await reportesService.GetReportesAsync(0, CargaInicial);

            if (respuesta == null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data == null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            reportesCargados.Clear();
            reportesCargados.AddRange(respuesta.Data);
            reportesSaltados = respuesta.Data.Count;
            elementosMostrados = 0;
            AplicarFiltrosReportesAdmin();
            await ReintentarReportesSiApiDisponible(false);
        }
        finally
        {
            cargandoReportesGenerales = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CargarMasReportes()
    {
        if (IsBusy || cargandoReportesGenerales)
        {
            return;
        }

        var reportesFiltrados = ObtenerReportesFiltrados();
        var mostroReportes = false;

        if (elementosMostrados < reportesFiltrados.Count)
        {
            MostrarSiguientesReportes();
            mostroReportes = true;
        }

        try
        {
            IsBusy = true;
            await CargarMasReportesDesdeApiAsync();

            if (!mostroReportes && elementosMostrados < ObtenerReportesFiltrados().Count)
            {
                MostrarSiguientesReportes();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CargarMasReportesDesdeApiAsync()
    {
        var respuesta = await reportesService.GetReportesAsync(reportesSaltados, CantidadVisible);

        if (respuesta == null)
        {
            Mensaje = "No se pudo conectar con la API.";
            return;
        }

        if (!respuesta.Success || respuesta.Data == null)
        {
            Mensaje = respuesta.Message;
            return;
        }

        reportesCargados.AddRange(respuesta.Data);
        reportesSaltados += respuesta.Data.Count;
    }

    private void MostrarSiguientesReportes()
    {
        var reportesParaMostrar = ObtenerReportesFiltrados().Skip(elementosMostrados).Take(CantidadVisible).ToList();

        foreach (var reporte in reportesParaMostrar)
        {
            Reportes.Add(reporte);
        }

        elementosMostrados += reportesParaMostrar.Count;
    }

    private void AplicarFiltrosReportesAdmin()
    {
        Reportes.Clear();
        elementosMostrados = 0;
        MostrarSiguientesReportes();
    }

    private List<ReporteGeneralDto> ObtenerReportesFiltrados()
    {
        var reportesFiltrados = reportesCargados.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(EstadoFiltroSeleccionado) && EstadoFiltroSeleccionado != "Todos")
        {
            reportesFiltrados = reportesFiltrados.Where(reporte => reporte.EstadoTexto == EstadoFiltroSeleccionado);
        }

        if (!string.IsNullOrWhiteSpace(CategoriaFiltroSeleccionada) && CategoriaFiltroSeleccionada != "Todos")
        {
            reportesFiltrados = reportesFiltrados.Where(reporte => reporte.CategoriaTexto == CategoriaFiltroSeleccionada);
        }

        return reportesFiltrados.ToList();
    }

    [RelayCommand]
    private async Task CargarMisReportes()
    {
        try
        {
            await CargarMisReportesAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task CargarMisReportesAsync()
    {
        if (cargandoReportesPropios)
        {
            return;
        }

        try
        {
            cargandoReportesPropios = true;
            IsBusy = true;
            Mensaje = "";
            await CargarNombreUsuarioAsync();

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontró la sesión del usuario.";
                return;
            }

            var respuesta = await reportesService.GetByUsuarioAsync(idUsuario, 0, CargaInicial);

            if (respuesta == null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data == null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            reportesPropiosCargados.Clear();
            reportesPropiosCargados.AddRange(respuesta.Data);
            MisReportes.Clear();
            reportesPropiosSaltados = respuesta.Data.Count;
            elementosPropiosMostrados = 0;
            MostrarSiguientesPropios();
            ActualizarConteoPerfil();
            await ReintentarReportesSiApiDisponible(false);
        }
        finally
        {
            cargandoReportesPropios = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CargarMasMisReportes()
    {
        if (IsBusy || cargandoReportesPropios)
        {
            return;
        }

        if (elementosPropiosMostrados < reportesPropiosCargados.Count)
        {
            MostrarSiguientesPropios();
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

            if (respuesta == null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data == null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            reportesPropiosCargados.AddRange(respuesta.Data);
            reportesPropiosSaltados += respuesta.Data.Count;
            MostrarSiguientesPropios();
            ActualizarConteoPerfil();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void MostrarSiguientesPropios()
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
    private async Task CargarPerfil()
    {
        if (cargandoPerfil)
        {
            return;
        }

        try
        {
            cargandoPerfil = true;
            await CargarNombreUsuarioAsync();
            await CargarReportesPendientesAsync();

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                TotalMisReportes = 0;
                ReportesAtendidos = 0;
                ReportesPendientesUsuario = 0;
                ActualizarTextoConteosPerfil();
                return;
            }

            var respuesta = await reportesService.GetByUsuarioAsync(idUsuario, 0, CargaInicial);

            if (respuesta == null || !respuesta.Success || respuesta.Data == null)
            {
                ActualizarConteoPerfil();
                return;
            }

            reportesPropiosCargados.Clear();
            reportesPropiosCargados.AddRange(respuesta.Data);
            ActualizarConteoPerfil();
        }
        finally
        {
            cargandoPerfil = false;
        }
    }

    private void ActualizarConteoPerfil()
    {
        var reportesParaContar = reportesPropiosCargados.Count > 0 ? reportesPropiosCargados : MisReportes.ToList();

        TotalMisReportes = reportesParaContar.Count;
        ReportesAtendidos = reportesParaContar.Count(x => x.IdEstado == (int)Estados.Resuelto);
        ReportesPendientesUsuario = reportesParaContar.Count(x => x.IdEstado == (int)Estados.Pendiente);
        ActualizarTextoConteosPerfil();
    }

    private void ActualizarTextoConteosPerfil()
    {
        TotalMisReportesTexto = ObtenerTextoConteo(TotalMisReportes);
        ReportesAtendidosTexto = ObtenerTextoConteo(ReportesAtendidos);
        ReportesPendientesUsuarioTexto = ObtenerTextoConteo(ReportesPendientesUsuario);
    }

    private string ObtenerTextoConteo(int cantidad)
    {
        if (cantidad >= CargaInicial)
        {
            return $"{CargaInicial}+";
        }

        return cantidad.ToString();
    }

    private string ValidacionReporte(string titulo, string descripcion, string? direccion)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            return "Ingrese un título para el reporte.";
        }

        if (titulo.Length < 3)
        {
            return "Ingrese un título con al menos 3 caracteres.";
        }

        if (titulo.Length > 120)
        {
            return "Ingrese un título con un máximo de 120 caracteres.";
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return "Ingrese una descripción para el reporte.";
        }

        if (descripcion.Length > 800)
        {
            return "Ingrese una descripción con un máximo de 800 caracteres.";
        }

        if (!string.IsNullOrWhiteSpace(direccion) && direccion.Length > 500)
        {
            return "Ingrese una dirección con un máximo de 500 caracteres.";
        }

        return "";
    }

    [RelayCommand]
    private async Task TomarFoto()
    {
        if (procesandoFoto)
        {
            return;
        }

        try
        {
            procesandoFoto = true;
            var resultado = await fotoService.TomarFotoAsync();

            if (resultado.RutaImagen == null || resultado.FotoBase64 == null)
            {
                Mensaje = resultado.Error ?? "No se pudo tomar la foto.";
                await MostrarToastAsync(Mensaje);
                return;
            }

            RutaImagen = resultado.RutaImagen;
            ImagenSeleccionada = ImageSource.FromFile(resultado.RutaImagen);
            MostrarTextoImagen = false;
            fotoBase64 = resultado.FotoBase64;
            Mensaje = "Fotografía tomada correctamente.";
            await MostrarToastAsync(Mensaje);
        }
        finally
        {
            procesandoFoto = false;
        }
    }

    [RelayCommand]
    private async Task SeleccionarFoto()
    {
        if (procesandoFoto)
        {
            return;
        }

        try
        {
            procesandoFoto = true;
            var resultado = await fotoService.SeleccionarFotoAsync();

            if (resultado.RutaImagen == null || resultado.FotoBase64 == null)
            {
                Mensaje = resultado.Error ?? "No se pudo seleccionar la foto.";
                await MostrarToastAsync(Mensaje);
                return;
            }

            RutaImagen = resultado.RutaImagen;
            ImagenSeleccionada = ImageSource.FromFile(resultado.RutaImagen);
            MostrarTextoImagen = false;
            fotoBase64 = resultado.FotoBase64;
            Mensaje = "Fotografía seleccionada correctamente.";
            await MostrarToastAsync(Mensaje);
        }
        finally
        {
            procesandoFoto = false;
        }
    }

    [RelayCommand]
    private async Task CrearReporte()
    {
        if (IsBusy)
        {
            return;
        }

        var mensajeValidacion = ValidacionReporte(TituloReporte, DescripcionReporte, "");

        if (!string.IsNullOrWhiteSpace(mensajeValidacion))
        {
            Mensaje = mensajeValidacion;
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = "";

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
                Foto = fotoBase64,
                IdUsuario = idUsuario,
                IdCategoria = GetIdCategoria(CategoriaSeleccionada),
                ClientRequestId = Guid.NewGuid().ToString()
            };

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await GuardarReportePendienteAsync(reporte);
                return;
            }

            var respuesta = await reportesService.CrearReporteAsync(reporte);

            if (respuesta == null)
            {
                var apiDisponible = await reportesService.ApiDisponibleAsync();

                if (!apiDisponible)
                {
                    await GuardarReportePendienteAsync(reporte);
                    return;
                }

                Mensaje = "La API responde, pero no pudo recibir el reporte. Revisa que la imagen no sea demasiado pesada.";
                await MostrarToastAsync(Mensaje);
                return;
            }

            if (!respuesta.Success || respuesta.Data == null)
            {
                Mensaje = respuesta.Message;
                await MostrarToastAsync(Mensaje);
                return;
            }

            await AgregarReporteLista(respuesta.Data);
            ActualizarConteoPerfil();

            TituloReporte = "";
            DescripcionReporte = "";
            CategoriaSeleccionada = "Bache";
            RutaImagen = null;
            ImagenSeleccionada = null;
            MostrarTextoImagen = true;
            fotoBase64 = null;
            Mensaje = respuesta.Message;
            await MostrarToastAsync("Reporte creado correctamente.");
            await ReintentarReportesSiApiDisponible(false);

            await Shell.Current.GoToAsync("//reportes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AgregarReporteLista(ReporteDetalleDto reporte)
    {
        var nombreUsuario = await authService.GetNombreUsuarioAsync() ?? "";

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

        ActualizarConteoPerfil();
    }

    private async Task GuardarReportePendienteAsync(SubirReporteDto reporte)
    {
        await reportePendienteService.GuardarReportePendienteAsync(reporte);
        await CargarReportesPendientesAsync();

        TituloReporte = "";
        DescripcionReporte = "";
        CategoriaSeleccionada = "Bache";
        RutaImagen = null;
        ImagenSeleccionada = null;
        MostrarTextoImagen = true;
        fotoBase64 = null;

        Mensaje = "No se pudo conectar con la API. El reporte se subirá cuando vuelva la conexión.";
        await MostrarToastAsync("Reporte guardado. Se subirá cuando vuelva la conexión.");
        await Shell.Current.GoToAsync("//reportes");
    }

    private async Task GuardarEdicionPendienteAsync(int idReporte, EditarReporteDto reporteEditado)
    {
        await reportePendienteService.GuardarEdicionPendienteAsync(idReporte, reporteEditado);
        await CargarReportesPendientesAsync();
        Mensaje = "No se pudo conectar con la API. La edición se enviará cuando vuelva la conexión.";
        await MostrarToastAsync("Edición guardada. Se enviará cuando vuelva la conexión.");
        await Shell.Current.GoToAsync("//misReportes");
    }

    private async Task GuardarEliminacionPendienteAsync(int idReporte, int idUsuario, string rutaRegreso)
    {
        await reportePendienteService.GuardarEliminacionPendienteAsync(idReporte, idUsuario);
        await CargarReportesPendientesAsync();
        QuitarReporteDeListas(idReporte);
        ReporteDetalle = null;
        Mensaje = "No se pudo conectar con la API. La eliminación se enviará cuando vuelva la conexión.";
        await MostrarToastAsync("Eliminación guardada. Se enviará cuando vuelva la conexión.");
        await Shell.Current.GoToAsync(rutaRegreso);
    }

    private async Task GuardarCambioEstadoPendienteAsync(int idReporte, CambiarEstadoReporteDto cambiarEstadoDto)
    {
        await reportePendienteService.GuardarCambioEstadoPendienteAsync(idReporte, cambiarEstadoDto);
        await CargarReportesPendientesAsync();
        Mensaje = "No se pudo conectar con la API. El cambio de estado se enviará cuando vuelva la conexión.";
        await MostrarToastAsync("Cambio de estado guardado. Se enviará cuando vuelva la conexión.");
        await Shell.Current.GoToAsync("//adminReportes");
    }
    private async Task CargarReportesPendientesAsync()
    {
        var accionesPendientes = await reportePendienteService.ObtenerAccionesPendientesAsync();
        ReportesPendientes = accionesPendientes.Count;
    }

    private async Task CargarNombreUsuarioAsync()
    {
        NombreUsuario = await authService.GetNombreUsuarioAsync() ?? "";
    }

    private async Task RevisarConexionAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            Mensaje = "Sin conexión a internet.";
            await MostrarToastAsync("Sin conexión a internet.");
            return;
        }

        var apiDisponible = await reportesService.ApiDisponibleAsync();

        if (!apiDisponible)
        {
            Mensaje = "Hay internet, pero no se pudo conectar con la API.";
            await MostrarToastAsync("No se pudo conectar con la API.");
            return;
        }

        await MostrarToastAsync("Conexión recuperada.");
        await ReintentarReportes(true);
    }

    private async Task ReintentarReportesSiApiDisponible(bool mostrarAlerta)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }

        var apiDisponible = await reportesService.ApiDisponibleAsync();

        if (!apiDisponible)
        {
            return;
        }

        await ReintentarReportes(mostrarAlerta);
    }

    private async Task ReintentarReportes(bool mostrarAlerta)
    {
        if (enviandoReportesPendientes)
        {
            return;
        }

        try
        {
            enviandoReportesPendientes = true;

            var accionesPendientes = await reportePendienteService.ObtenerAccionesPendientesAsync();

            if (accionesPendientes.Count == 0)
            {
                ReportesPendientes = 0;
                return;
            }

            var enviados = 0;

            foreach (var accionPendiente in accionesPendientes)
            {
                var respuesta = await EnviarAccionPendienteAsync(accionPendiente);

                if (respuesta == null)
                {
                    var apiDisponible = await reportesService.ApiDisponibleAsync();

                    if (apiDisponible)
                    {
                        Mensaje = "La API responde, pero un pendiente no pudo enviarse. Revisa que la imagen no sea demasiado pesada.";

                        if (mostrarAlerta)
                        {
                            await MostrarToastAsync(Mensaje);
                        }
                    }

                    continue;
                }

                if (!string.IsNullOrWhiteSpace(respuesta.Message) && respuesta.Message.Contains("ya fue registrado", StringComparison.OrdinalIgnoreCase))
                {
                    await reportePendienteService.EliminarAccionPendienteAsync(accionPendiente.IdPendiente);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(respuesta.Message) && (respuesta.Message.Contains("demasiado grande", StringComparison.OrdinalIgnoreCase) || respuesta.Message.Contains("formato", StringComparison.OrdinalIgnoreCase) || respuesta.Message.Contains("base64", StringComparison.OrdinalIgnoreCase) || respuesta.Message.Contains("Categoria", StringComparison.OrdinalIgnoreCase) || respuesta.Message.Contains("Reporte no encontrado", StringComparison.OrdinalIgnoreCase) || respuesta.Message.Contains("Acción pendiente no válida", StringComparison.OrdinalIgnoreCase) || respuesta.Message.Contains("No puedes", StringComparison.OrdinalIgnoreCase) || respuesta.Message.Contains("Solo un administrador", StringComparison.OrdinalIgnoreCase)))
                {

                    await reportePendienteService.EliminarAccionPendienteAsync(accionPendiente.IdPendiente);
                    Mensaje = respuesta.Message;
                    await MostrarToastAsync(respuesta.Message);
                    continue;
                }

                if (!respuesta.Success || respuesta.Data == null)
                {
                    continue;
                }

                await reportePendienteService.EliminarAccionPendienteAsync(accionPendiente.IdPendiente);
                await AplicarAccionPendienteEnListasAsync(accionPendiente, respuesta.Data);
                ActualizarConteoPerfil();
                enviados++;
            }

            await CargarReportesPendientesAsync();

            if (enviados > 0)
            {
                Mensaje = $"Se procesaron {enviados} pendientes.";

                if (mostrarAlerta)
                {
                    await MostrarToastAsync($"Se procesaron {enviados} pendientes.");
                }
            }
        }
        finally
        {
            enviandoReportesPendientes = false;
        }
    }

    private async Task<ApiResponse<ReporteDetalleDto>?> EnviarAccionPendienteAsync(AccionPendienteDto accionPendiente)
    {
        if (accionPendiente.Tipo == "Crear" && accionPendiente.SubirReporte != null)
        {
            return await reportesService.CrearReporteAsync(accionPendiente.SubirReporte);
        }

        if (accionPendiente.Tipo == "Editar" && accionPendiente.EditarReporte != null)
        {
            return await reportesService.EditarReporteAsync(accionPendiente.IdReporte, accionPendiente.EditarReporte);
        }

        if (accionPendiente.Tipo == "Eliminar")
        {
            return await reportesService.EliminarReporteAsync(accionPendiente.IdReporte, accionPendiente.IdUsuario);
        }

        if (accionPendiente.Tipo == "CambiarEstado" && accionPendiente.CambiarEstado != null)
        {
            return await reportesService.CambiarEstadoAsync(accionPendiente.IdReporte, accionPendiente.CambiarEstado);
        }

        return new ApiResponse<ReporteDetalleDto>
        {
            Success = false,
            Message = "Acción pendiente no válida."
        };
    }

    private async Task AplicarAccionPendienteEnListasAsync(AccionPendienteDto accionPendiente, ReporteDetalleDto? reporte)
    {
        if (accionPendiente.Tipo == "Crear" && reporte != null)
        {
            await AgregarReporteLista(reporte);
            return;
        }

        if ((accionPendiente.Tipo == "Editar" || accionPendiente.Tipo == "CambiarEstado") && reporte != null)
        {
            ActualizarReporteEnListas(reporte);
            return;
        }

        if (accionPendiente.Tipo == "Eliminar")
        {
            QuitarReporteDeListas(accionPendiente.IdReporte);
        }
    }


    private async Task MostrarToastAsync(string mensaje)
    {
        await alertaService.MostrarToastAsync(mensaje);
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
            Mensaje = "";

            var respuesta = await reportesService.GetReporteAsync(idReporte);

            if (respuesta == null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success || respuesta.Data == null)
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

            if (ruta == "reporteAdmin")
            {
                await Task.Delay(50);
                estadoOriginalReporte = EstadoSeleccionado;
                estadoReporteFueModificado = false;
                PuedeConfirmarCambioEstado = false;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PrepararDetalleReporte(ReporteDetalleDto reporte, bool puedeEditar)
    {
        preparandoDetalleReporte = true;
        ReporteDetalle = reporte;
        PuedeEditarReporte = puedeEditar;
        EstaEditandoReporte = false;
        CamposSoloLectura = true;
        TituloDetalle = reporte.Titulo;
        DescripcionDetalle = reporte.Descripcion;
        CategoriaDetalle = GetTextoCategoria(reporte.IdCategoria);
        estadoOriginalReporte = GetTextoEstado(reporte.IdEstado);
        EstadoSeleccionado = GetTextoEstado(reporte.IdEstado);
        estadoReporteFueModificado = false;
        PuedeConfirmarCambioEstado = false;
        preparandoDetalleReporte = false;
        ImagenDetalleUrl = reportesService.ObtenerUrlImagen(reporte.ImgUrl);
        RutaImagen = null;
        ImagenSeleccionada = null;
        MostrarTextoImagen = true;
        fotoBase64 = null;
    }

    private string GetTextoCategoria(int idCategoria)
    {
        if (idCategoria == (int)Categorias.Bache) return "Bache";
        if (idCategoria == (int)Categorias.Fuga_de_agua) return "Fuga de agua";
        if (idCategoria == (int)Categorias.Basura) return "Basura";
        if (idCategoria == (int)Categorias.Alumbrado_Publico) return "Alumbrado publico";
        if (idCategoria == (int)Categorias.Accidente) return "Accidente";
        if (idCategoria == (int)Categorias.Otro) return "Otro";
        return "Bache";
    }

    private int GetIdCategoria(string categoria)
    {
        if (categoria == "Bache") return (int)Categorias.Bache;
        if (categoria == "Fuga de agua") return (int)Categorias.Fuga_de_agua;
        if (categoria == "Basura") return (int)Categorias.Basura;
        if (categoria == "Alumbrado publico") return (int)Categorias.Alumbrado_Publico;
        if (categoria == "Accidente") return (int)Categorias.Accidente;
        if (categoria == "Otro") return (int)Categorias.Otro;
        return (int)Categorias.Bache;
    }

    private string GetTextoEstado(int idEstado)
    {
        if (idEstado == (int)Estados.Pendiente) return "Pendiente";
        if (idEstado == (int)Estados.En_Progreso) return "En progreso";
        if (idEstado == (int)Estados.Resuelto) return "Resuelto";
        return "Pendiente";
    }

    private int GetIdEstado(string estado)
    {
        if (estado == "Pendiente") return (int)Estados.Pendiente;
        if (estado == "En progreso") return (int)Estados.En_Progreso;
        if (estado == "Resuelto") return (int)Estados.Resuelto;
        return (int)Estados.Pendiente;
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
        Mensaje = "";
    }

    [RelayCommand]
    private async Task GuardarEdicionReporte()
    {
        if (IsBusy || ReporteDetalle == null)
        {
            return;
        }

        var mensajeValidacion = ValidacionReporte(TituloDetalle, DescripcionDetalle, ReporteDetalle.Direccion);

        if (!string.IsNullOrWhiteSpace(mensajeValidacion))
        {
            Mensaje = mensajeValidacion;
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = "";

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontró la sesion del usuario.";
                return;
            }

            var reporteEditado = new EditarReporteDto
            {
                Id = ReporteDetalle.Id,
                Titulo = TituloDetalle.Trim(),
                Descripcion = DescripcionDetalle.Trim(),
                Direccion = ReporteDetalle.Direccion,
                Foto = fotoBase64,
                IdUsuario = idUsuario,
                IdCategoria = GetIdCategoria(CategoriaDetalle)
            };

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await GuardarEdicionPendienteAsync(ReporteDetalle.Id, reporteEditado);
                return;
            }

            var respuesta = await reportesService.EditarReporteAsync(ReporteDetalle.Id, reporteEditado);

            if (respuesta == null)
            {
                var apiDisponible = await reportesService.ApiDisponibleAsync();

                if (!apiDisponible)
                {
                    await GuardarEdicionPendienteAsync(ReporteDetalle.Id, reporteEditado);
                    return;
                }

                Mensaje = "La API responde, pero no pudo editar el reporte.";
                await MostrarToastAsync(Mensaje);
                return;
            }

            if (!respuesta.Success || respuesta.Data == null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            PrepararDetalleReporte(respuesta.Data, true);
            ActualizarReporteEnListas(respuesta.Data);
            Mensaje = respuesta.Message;
            await MostrarToastAsync("Reporte editado correctamente.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EliminarReporte()
    {
        if (IsBusy || ReporteDetalle == null)
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
            Mensaje = "";

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontró la sesion del usuario.";
                return;
            }

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await GuardarEliminacionPendienteAsync(ReporteDetalle.Id, idUsuario, "//misReportes");
                return;
            }

            var respuesta = await reportesService.EliminarReporteAsync(ReporteDetalle.Id, idUsuario);

            if (respuesta == null)
            {
                var apiDisponible = await reportesService.ApiDisponibleAsync();

                if (!apiDisponible)
                {
                    await GuardarEliminacionPendienteAsync(ReporteDetalle.Id, idUsuario, "//misReportes");
                    return;
                }

                Mensaje = "La API responde, pero no pudo eliminar el reporte.";
                await MostrarToastAsync(Mensaje);
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
            await MostrarToastAsync("Reporte eliminado correctamente.");
            await Shell.Current.GoToAsync("//misReportes");
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

        if (reportePropio != null)
        {
            var posicion = MisReportes.IndexOf(reportePropio);
            MisReportes[posicion] = reportePropioActualizado;
        }

        var reportePropioCargado = reportesPropiosCargados.FirstOrDefault(x => x.Id == reporte.Id);

        if (reportePropioCargado != null)
        {
            var posicion = reportesPropiosCargados.IndexOf(reportePropioCargado);
            reportesPropiosCargados[posicion] = reportePropioActualizado;
        }

        var reporteGeneral = Reportes.FirstOrDefault(x => x.Id == reporte.Id);

        if (reporteGeneral != null)
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

            if (reporteGeneralCargado != null)
            {
                var posicionCargado = reportesCargados.IndexOf(reporteGeneralCargado);
                reportesCargados[posicionCargado] = reporteGeneralActualizado;
            }
        }

        ActualizarConteoPerfil();
    }

    private void QuitarReporteDeListas(int idReporte)
    {
        var reportePropio = MisReportes.FirstOrDefault(x => x.Id == idReporte);

        if (reportePropio != null)
        {
            MisReportes.Remove(reportePropio);
        }

        var reportePropioCargado = reportesPropiosCargados.FirstOrDefault(x => x.Id == idReporte);

        if (reportePropioCargado != null)
        {
            reportesPropiosCargados.Remove(reportePropioCargado);
        }

        var reporteGeneral = Reportes.FirstOrDefault(x => x.Id == idReporte);

        if (reporteGeneral != null)
        {   
            Reportes.Remove(reporteGeneral);
        }

        var reporteGeneralCargado = reportesCargados.FirstOrDefault(x => x.Id == idReporte);

        if (reporteGeneralCargado != null)
        {
            reportesCargados.Remove(reporteGeneralCargado);
        }

        ActualizarConteoPerfil();
    }

    [RelayCommand]
    private async Task EliminarReporteAdmin()
    {
        if (IsBusy || ReporteDetalle == null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlert("Eliminar reporte", "Está seguro que quiere eliminar este reporte?", "Aceptar", "Cancelar");

        if (!confirmar)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = "";

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontró la sesión del usuario.";
                return;
            }

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await GuardarEliminacionPendienteAsync(ReporteDetalle.Id, idUsuario, "//adminReportes");
                return;
            }

            var respuesta = await reportesService.EliminarReporteAsync(ReporteDetalle.Id, idUsuario);

            if (respuesta == null)
            {
                var apiDisponible = await reportesService.ApiDisponibleAsync();

                if (!apiDisponible)
                {
                    await GuardarEliminacionPendienteAsync(ReporteDetalle.Id, idUsuario, "//adminReportes");
                    return;
                }

                Mensaje = "La API responde, pero no pudo eliminar el reporte.";
                await MostrarToastAsync(Mensaje);
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
            await MostrarToastAsync("Reporte eliminado correctamente.");
            await Shell.Current.GoToAsync("//adminReportes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CambiarEstadoReporte()
    {
        if (IsBusy || ReporteDetalle == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = "";

            var idUsuario = await authService.GetIdUsuarioAsync();

            if (idUsuario == 0)
            {
                Mensaje = "No se encontró la sesión del usuario.";
                return;
            }

            var cambiarEstadoDto = new CambiarEstadoReporteDto
            {
                IdEstado = GetIdEstado(EstadoSeleccionado),
                IdUsuario = idUsuario
            };

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                await GuardarCambioEstadoPendienteAsync(ReporteDetalle.Id, cambiarEstadoDto);
                return;
            }

            var respuesta = await reportesService.CambiarEstadoAsync(ReporteDetalle.Id, cambiarEstadoDto);

            if (respuesta == null)
            {
                var apiDisponible = await reportesService.ApiDisponibleAsync();

                if (!apiDisponible)
                {
                    await GuardarCambioEstadoPendienteAsync(ReporteDetalle.Id, cambiarEstadoDto);
                    return;
                }

                Mensaje = "La API responde, pero no pudo cambiar el estado.";
                await MostrarToastAsync(Mensaje);
                return;
            }

            if (!respuesta.Success || respuesta.Data == null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            PrepararDetalleReporte(respuesta.Data, false);
            ActualizarReporteEnListas(respuesta.Data);
            Mensaje = respuesta.Message;
            await MostrarToastAsync("Estado actualizado correctamente.");
            await Shell.Current.GoToAsync("//adminReportes");
        }
        finally
        {
            IsBusy = false;
        }
    }


    //Navegacion
    private async Task NavegarAsync(string ruta)
    {
        if (estaNavegando)
        {
            return;
        }

        try
        {
            estaNavegando = true;
            IsBusy = true;
            await Task.Delay(50);
            await Shell.Current.GoToAsync(ruta);
        }
        finally
        {
            IsBusy = false;
            estaNavegando = false;
        }
    }

    [RelayCommand]
    private async Task VolverReporte()
    {
        if (PuedeEditarReporte)
        {
            await NavegarAsync("//misReportes");
            return;
        }

        await NavegarAsync("//reportes");
    }

    [RelayCommand]
    private async Task IrInicio()
    {
        await NavegarAsync("//reportes");
    }

    [RelayCommand]
    private async Task IrAgregarReporte()
    {
        await NavegarAsync("//agregarReporte");
    }

    [RelayCommand]
    private async Task IrMisReportes()
    {
        await NavegarAsync("//misReportes");
    }

    [RelayCommand]
    private async Task IrPerfil()
    {
        await NavegarAsync("//perfil");
    }

    [RelayCommand]
    private async Task IrAdminReportes()
    {
        await NavegarAsync("//adminReportes");
    }

    [RelayCommand]
    private async Task Logout()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await authService.CerrarSesionAsync();
            reportesCargados.Clear();
            reportesPropiosCargados.Clear();
            Reportes.Clear();
            MisReportes.Clear();
            ReporteDetalle = null;
            NombreUsuario = "";
            TotalMisReportes = 0;
            ReportesAtendidos = 0;
            ReportesPendientesUsuario = 0;
            ActualizarTextoConteosPerfil();
            reportesSaltados = 0;
            reportesPropiosSaltados = 0;
            elementosMostrados = 0;
            elementosPropiosMostrados = 0;
            EstadoFiltroSeleccionado = "Todos";
            CategoriaFiltroSeleccionada = "Todos";
            Mensaje = "";

            await Shell.Current.GoToAsync("//login");
        }
        finally
        {
            IsBusy = false;
        }
    }

}
