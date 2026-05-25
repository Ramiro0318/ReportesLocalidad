using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportesLocalidadApp.Models.DTOs;
using ReportesLocalidadApp.Services;

namespace ReportesLocalidadApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ReportesService reportesService;
    private readonly List<ReporteGeneralDto> reportesCargados = new();
    private int reportesSaltados;
    private int elementosMostrados;
    private const int CargaInicial = 50;
    private const int CantidadVisible = 25;

    public MainViewModel(ReportesService reportesService)
    {
        this.reportesService = reportesService;
        Reportes = new ObservableCollection<ReporteGeneralDto>();
    }

    public ObservableCollection<ReporteGeneralDto> Reportes { get; }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string mensaje = string.Empty;

    [RelayCommand]
    private async Task CargarReportesAdmin()
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
