using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class InicioView : ContentPage
{
	public InicioView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}    
}
