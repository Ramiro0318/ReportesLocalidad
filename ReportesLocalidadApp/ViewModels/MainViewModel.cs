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

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string mensaje = string.Empty;

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
