using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportesLocalidadApp.Services;

namespace ReportesLocalidadApp.ViewModels;

public partial class RegistroViewModel : MainViewModel
{
    private readonly AuthService authService;

    [ObservableProperty]
    private string nombreUsuario = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmarPassword = string.Empty;

    public RegistroViewModel(AuthService authService)
    {
        this.authService = authService;
    }

    [RelayCommand]
    private async Task IrLogin()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Registrar()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Password))
        {
            Mensaje = "Ingrese usuario y contrasena.";
            return;
        }

        if (Password != ConfirmarPassword)
        {
            Mensaje = "Las contrasenas no coinciden.";
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            var respuesta = await authService.RegistrarAsync(NombreUsuario, Password);

            if (respuesta is null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            Mensaje = respuesta.Message;

            if (respuesta.Success)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
