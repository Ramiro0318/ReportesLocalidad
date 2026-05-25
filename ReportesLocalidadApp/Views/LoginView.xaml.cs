using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class LoginView : ContentPage
{
	public LoginView(AuthViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}   
}
