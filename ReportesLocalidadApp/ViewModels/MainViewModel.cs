using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReportesLocalidadApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string mensaje = string.Empty;

    [RelayCommand]
    private async Task IrInicio()
    {
        await Shell.Current.GoToAsync("//reportes");
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
        await Shell.Current.GoToAsync("//adminReportes");
    }
}
