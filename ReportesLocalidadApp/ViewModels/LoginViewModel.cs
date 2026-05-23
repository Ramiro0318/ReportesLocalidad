using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportesLocalidadApp.Services;

namespace ReportesLocalidadApp.ViewModels;

public partial class LoginViewModel : MainViewModel
{
    private readonly AuthService authService;

    [ObservableProperty]
    private string nombreUsuario = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    public LoginViewModel(AuthService authService)
    {
        this.authService = authService;
    }

    [RelayCommand]
    private async Task IrRegistro()
    {
        await Shell.Current.GoToAsync("registro");
    }

    [RelayCommand]
    private async Task Login()
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

        try
        {
            IsBusy = true;
            Mensaje = string.Empty;

            var respuesta = await authService.LoginAsync(NombreUsuario, Password);

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

            Mensaje = respuesta.Message;

            if (respuesta.Data.IdRol == 2)
            {
                await Shell.Current.GoToAsync("//adminReportes");
            }
            else
            {
                await Shell.Current.GoToAsync("//reportes");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
