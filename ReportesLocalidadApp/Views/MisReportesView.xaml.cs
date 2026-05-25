using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class MisReportesView : ContentPage
{
	public MisReportesView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}   
}
