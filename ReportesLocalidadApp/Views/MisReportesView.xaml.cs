using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class MisReportesView : ContentPage
{
	public MisReportesView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}   

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel viewModel && viewModel.MisReportes.Count == 0)
        {
            await Task.Delay(100);
            await viewModel.CargarMisReportesCommand.ExecuteAsync(null);
        }
    }
}
