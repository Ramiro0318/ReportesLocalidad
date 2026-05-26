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
        _ = CargarReportesPendientesAsync();
        _ = ReintentarReportesSiApiDisponible(false);
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

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string mensaje = string.Empty;

    [ObservableProperty]
    private int reportesPendientes;

    [ObservableProperty]
    private string tituloReporte = string.Empty;

    [ObservableProperty]
    private string descripcionReporte = string.Empty;

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
    private string tituloDetalle = string.Empty;

    [ObservableProperty]
    private string descripcionDetalle = string.Empty;

    [ObservableProperty]
    private string categoriaDetalle = "Bache";

    [ObservableProperty]
    private string estadoSeleccionado = "Pendiente";

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
            await ReintentarReportesSiApiDisponible(false);
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
            MostrarSiguientesPropios();
            await ReintentarReportesSiApiDisponible(false);
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
            MostrarSiguientesPropios();
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
    private async Task TomarFoto()
    {
        var resultado = await fotoService.TomarFotoAsync();

        if (resultado is null)
        {
            Mensaje = "No se pudo tomar la foto.";
            await MostrarToastAsync(Mensaje);
            return;
        }

        RutaImagen = resultado.Value.RutaImagen;
        ImagenSeleccionada = ImageSource.FromFile(resultado.Value.RutaImagen);
        MostrarTextoImagen = false;
        fotoBase64 = resultado.Value.FotoBase64;
        Mensaje = "Fotografia tomada correctamente.";
        await MostrarToastAsync(Mensaje);
    }

    [RelayCommand]
    private async Task SeleccionarFoto()
    {
        var resultado = await fotoService.SeleccionarFotoAsync();

        if (resultado is null)
        {
            Mensaje = "No se pudo seleccionar la foto.";
            await MostrarToastAsync(Mensaje);
            return;
        }

        RutaImagen = resultado.Value.RutaImagen;
        ImagenSeleccionada = ImageSource.FromFile(resultado.Value.RutaImagen);
        MostrarTextoImagen = false;
        fotoBase64 = resultado.Value.FotoBase64;
        Mensaje = "Fotografia seleccionada correctamente.";
        await MostrarToastAsync(Mensaje);
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
                Foto = fotoBase64,
                IdUsuario = idUsuario,
                IdCategoria = ObtenerIdCategoria(CategoriaSeleccionada),
                ClientRequestId = Guid.NewGuid().ToString()
            };

            var apiDisponible = await reportesService.ApiDisponibleAsync();

            if (!apiDisponible)
            {
                await GuardarReportePendienteAsync(reporte);
                return;
            }

            var respuesta = await reportesService.CrearReporteAsync(reporte);

            if (respuesta is null)
            {
                await GuardarReportePendienteAsync(reporte);
                return;
            }

            if (!respuesta.Success || respuesta.Data is null)
            {
                Mensaje = respuesta.Message;
                return;
            }

            await AgregarReporteLista(respuesta.Data);

            TituloReporte = string.Empty;
            DescripcionReporte = string.Empty;
            CategoriaSeleccionada = "Bache";
            RutaImagen = null;
            ImagenSeleccionada = null;
            MostrarTextoImagen = true;
            fotoBase64 = null;
            Mensaje = respuesta.Message;
            await MostrarToastAsync("Reporte creado correctamente.");
            await ReintentarReportesSiApiDisponible(false);

            await Shell.Current.GoToAsync("reportes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AgregarReporteLista(ReporteDetalleDto reporte)
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

    private async Task GuardarReportePendienteAsync(SubirReporteDto reporte)
    {
        await reportePendienteService.GuardarReportePendienteAsync(reporte);
        await CargarReportesPendientesAsync();

        TituloReporte = string.Empty;
        DescripcionReporte = string.Empty;
        CategoriaSeleccionada = "Bache";
        RutaImagen = null;
        ImagenSeleccionada = null;
        MostrarTextoImagen = true;
        fotoBase64 = null;

        Mensaje = "No se pudo conectar con la API. El reporte se subira cuando vuelva la conexion.";
        await MostrarToastAsync("Reporte guardado. Se subira cuando vuelva la conexion.");
        await Shell.Current.GoToAsync("reportes");
    }

    private async Task CargarReportesPendientesAsync()
    {
        var reportesPendientesLocales = await reportePendienteService.ObtenerReportesPendientesAsync();
        ReportesPendientes = reportesPendientesLocales.Count;
    }

    private async Task RevisarConexionAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            Mensaje = "Sin conexion a internet.";
            await MostrarToastAsync("Sin conexion a internet.");
            return;
        }

        var apiDisponible = await reportesService.ApiDisponibleAsync();

        if (!apiDisponible)
        {
            Mensaje = "Hay internet, pero no se pudo conectar con la API.";
            await MostrarToastAsync("No se pudo conectar con la API.");
            return;
        }

        await MostrarToastAsync("Conexion recuperada.");
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

            var reportesPendientesLocales = await reportePendienteService.ObtenerReportesPendientesAsync();

            if (reportesPendientesLocales.Count == 0)
            {
                ReportesPendientes = 0;
                return;
            }

            var enviados = 0;

            foreach (var reportePendiente in reportesPendientesLocales)
            {
                var respuesta = await reportesService.CrearReporteAsync(reportePendiente);

                if (respuesta is null)
                {
                    continue;
                }

                if (ReporteDuplicado(respuesta.Message))
                {
                    await reportePendienteService.EliminarReportePendienteAsync(reportePendiente.ClientRequestId);
                    continue;
                }

                if (!respuesta.Success || respuesta.Data is null)
                {
                    continue;
                }

                await reportePendienteService.EliminarReportePendienteAsync(reportePendiente.ClientRequestId);
                await AgregarReporteLista(respuesta.Data);
                enviados++;
            }

            await CargarReportesPendientesAsync();

            if (enviados > 0)
            {
                Mensaje = $"Se enviaron {enviados} reportes pendientes.";

                if (mostrarAlerta)
                {
                    await MostrarToastAsync($"Se enviaron {enviados} reportes pendientes.");
                }
            }
        }
        finally
        {
            enviandoReportesPendientes = false;
        }
    }

    private bool ReporteDuplicado(string? mensaje)
    {
        return !string.IsNullOrWhiteSpace(mensaje) &&
            mensaje.Contains("ya fue registrado", StringComparison.OrdinalIgnoreCase);
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
        CategoriaDetalle = ObtenerTextoCategoria(reporte.IdCategoria);
        EstadoSeleccionado = ObtenerTextoEstado(reporte.IdEstado);
        ImagenDetalleUrl = reportesService.ObtenerUrlImagen(reporte.ImgUrl);
        RutaImagen = null;
        ImagenSeleccionada = null;
        MostrarTextoImagen = true;
        fotoBase64 = null;
    }

    private string ObtenerTextoCategoria(int idCategoria)
    {
        if (idCategoria == (int)Categorias.Bache) return "Bache";
        if (idCategoria == (int)Categorias.Fuga_de_agua) return "Fuga de agua";
        if (idCategoria == (int)Categorias.Basura) return "Basura";
        if (idCategoria == (int)Categorias.Alumbrado_Publico) return "Alumbrado publico";
        if (idCategoria == (int)Categorias.Accidente) return "Accidente";
        if (idCategoria == (int)Categorias.Otro) return "Otro";
        return "Bache";
    }

    private int ObtenerIdCategoria(string categoria)
    {
        if (categoria == "Bache") return (int)Categorias.Bache;
        if (categoria == "Fuga de agua") return (int)Categorias.Fuga_de_agua;
        if (categoria == "Basura") return (int)Categorias.Basura;
        if (categoria == "Alumbrado publico") return (int)Categorias.Alumbrado_Publico;
        if (categoria == "Accidente") return (int)Categorias.Accidente;
        if (categoria == "Otro") return (int)Categorias.Otro;
        return (int)Categorias.Bache;
    }

    private string ObtenerTextoEstado(int idEstado)
    {
        if (idEstado == (int)Estados.Pendiente) return "Pendiente";
        if (idEstado == (int)Estados.En_Progreso) return "En progreso";
        if (idEstado == (int)Estados.Resuelto) return "Resuelto";
        return "Pendiente";
    }

    private int ObtenerIdEstado(string estado)
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
                Foto = fotoBase64,
                IdUsuario = idUsuario,
                IdCategoria = ObtenerIdCategoria(CategoriaDetalle)
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
            await MostrarToastAsync("Reporte eliminado correctamente.");
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
    private async Task EliminarReporteAdmin()
    {
        if (IsBusy || ReporteDetalle is null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlert("Eliminar reporte", "Esta seguro que quiere eliminar este reporte?", "Aceptar", "Cancelar");

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
            await MostrarToastAsync("Reporte eliminado correctamente.");
            await Shell.Current.GoToAsync("adminReportes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CambiarEstadoReporte()
    {
        if (IsBusy || ReporteDetalle is null)
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

            var cambiarEstadoDto = new CambiarEstadoReporteDto
            {
                IdEstado = ObtenerIdEstado(EstadoSeleccionado),
                IdUsuario = idUsuario
            };

            var respuesta = await reportesService.CambiarEstadoAsync(ReporteDetalle.Id, cambiarEstadoDto);

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

            PrepararDetalleReporte(respuesta.Data, false);
            ActualizarReporteEnListas(respuesta.Data);
            Mensaje = respuesta.Message;
            await MostrarToastAsync("Estado actualizado correctamente.");
            await Shell.Current.GoToAsync("adminReportes");
        }
        finally
        {
            IsBusy = false;
        }
    }


    //Navegacion
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