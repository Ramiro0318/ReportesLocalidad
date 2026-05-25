using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class ReportesAdminView : ContentPage
{
	public ReportesAdminView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
