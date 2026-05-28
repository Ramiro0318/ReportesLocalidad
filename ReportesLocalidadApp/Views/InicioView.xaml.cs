using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class InicioView : ContentPage
{
	public InicioView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}    

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel viewModel && viewModel.Reportes.Count == 0)
        {
            await Task.Delay(100);
            await viewModel.CargarReportesCommand.ExecuteAsync(null);
        }
    }
}
