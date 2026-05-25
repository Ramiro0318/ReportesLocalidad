using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class ReporteAdminView : ContentPage
{
	public ReporteAdminView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
