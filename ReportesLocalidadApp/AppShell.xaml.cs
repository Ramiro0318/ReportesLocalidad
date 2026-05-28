using ReportesLocalidadApp.Views;

namespace ReportesLocalidadApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegistrarRutas();
        }

        private static void RegistrarRutas()
        {
            Routing.RegisterRoute("registro", typeof(RegistroView));
            Routing.RegisterRoute("reporte", typeof(ReporteView));
            Routing.RegisterRoute("reporteAdmin", typeof(ReporteAdminView));
        }
    }
}
