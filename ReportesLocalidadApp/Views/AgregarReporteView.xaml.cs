using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class AgregarReporteView : ContentPage
{
	public AgregarReporteView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
