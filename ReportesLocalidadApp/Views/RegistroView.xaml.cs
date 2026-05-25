using ReportesLocalidadApp.ViewModels;

namespace ReportesLocalidadApp.Views;

public partial class RegistroView : ContentPage
{
    public RegistroView(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
