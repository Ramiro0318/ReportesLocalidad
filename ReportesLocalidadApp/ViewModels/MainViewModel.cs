using CommunityToolkit.Mvvm.ComponentModel;

namespace ReportesLocalidadApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string mensaje = string.Empty;
}
