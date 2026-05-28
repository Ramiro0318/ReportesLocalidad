using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class ReporteView : ContentPage
{
	public ReporteView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
