using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class LoginView : ContentPage
{
	public LoginView(AuthViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}   

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AuthViewModel viewModel)
        {
            await viewModel.RevisarSesionGuardadaCommand.ExecuteAsync(null);
        }
    }
}
