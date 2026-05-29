using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class ReporteView : ContentPage
{
	public ReporteView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
