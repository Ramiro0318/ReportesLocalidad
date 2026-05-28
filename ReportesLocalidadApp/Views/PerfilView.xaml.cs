using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class PerfilView : ContentPage
{
	public PerfilView(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel viewModel)
        {
            await Task.Delay(100);
            await viewModel.CargarPerfilCommand.ExecuteAsync(null);
        }
    }
}
