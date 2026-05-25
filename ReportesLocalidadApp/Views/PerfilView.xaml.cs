using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class PerfilView : ContentPage
{
	public PerfilView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
