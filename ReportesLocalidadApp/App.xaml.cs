using Microsoft.Extensions.DependencyInjection;
using ReportesLocalidadApp.Views;

namespace ReportesLocalidadApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();          
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}