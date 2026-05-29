using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportesLocalidadApp.Services;

namespace ReportesLocalidadApp.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly AuthService authService;
    private bool sesionRevisada;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string mensaje = "";

    [ObservableProperty]
    private string nombreUsuario = "";

    [ObservableProperty]
    private string password = "";

    [ObservableProperty]
    private string confirmarPassword = "";

    public AuthViewModel(AuthService authService)
    {
        this.authService = authService;
    }

    [RelayCommand]
    private async Task RevisarSesionGuardada()
    {
        if (sesionRevisada)
        {
            return;
        }

        try
        {
            sesionRevisada = true;
            IsBusy = true;
            Mensaje = "";

            var sesion = await authService.ObtenerSesionGuardadaAsync();

            if (sesion is null)
            {
                return;
            }

            if (sesion.IdRol == 2)
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

    [RelayCommand]
    private async Task IrRegistro()
    {
        LimpiarCampos();
        await Shell.Current.GoToAsync("registro");
    }

    [RelayCommand]
    private async Task IrLogin()
    {
        LimpiarCampos();
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Login()
    {
        if (IsBusy)
        {
            return;
        }

        var mensajeValidacion = ValidarUsuario();

        if (!string.IsNullOrWhiteSpace(mensajeValidacion))
        {
            Mensaje = mensajeValidacion;
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = "";

            var respuesta = await authService.LoginAsync(NombreUsuario, Password);

            if (respuesta == null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            if (!respuesta.Success)
            {
                Mensaje = respuesta.Message;
                return;
            }

            Mensaje = respuesta.Message;

            if (respuesta.Data!.IdRol == 2)
            {
                LimpiarCampos();
                await Shell.Current.GoToAsync("//adminReportes");
            }
            else
            {
                LimpiarCampos();
                await Shell.Current.GoToAsync("//reportes");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Registrar()
    {
        if (IsBusy)
        {
            return;
        }

        var mensajeValidacion = ValidarUsuario();

        if (!string.IsNullOrWhiteSpace(mensajeValidacion))
        {
            Mensaje = mensajeValidacion;
            return;
        }

        if (Password != ConfirmarPassword)
        {
            Mensaje = "Las contraseñas no coinciden.";
            return;
        }

        try
        {
            IsBusy = true;
            Mensaje = "";

            var respuesta = await authService.RegistrarAsync(NombreUsuario, Password);

            if (respuesta == null)
            {
                Mensaje = "No se pudo conectar con la API.";
                return;
            }

            Mensaje = respuesta.Message;

            if (respuesta.Success)
            {
                LimpiarCampos();
                await Shell.Current.GoToAsync("..");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LimpiarCampos()
    {
        NombreUsuario = "";
        Password = "";
        ConfirmarPassword = "";
        Mensaje = "";
    }

    private string ValidarUsuario()
    {
        var usuario = NombreUsuario.Trim();

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(Password))
        {
            return "Ingrese usuario y contraseña.";
        }

        if (usuario.Length > 40)
        {
            return "El usuario no puede tener más de 40 caracteres.";
        }

        if (Password.Length < 6 || Password.Length > 30)
        {
            return "La contraseña debe tener entre 6 y 30 caracteres.";
        }

        return "";
    }



}
