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
            Routing.RegisterRoute("reportes", typeof(InicioView));
            Routing.RegisterRoute("adminReportes", typeof(ReportesAdminView));
            Routing.RegisterRoute("registro", typeof(RegistroView));
            Routing.RegisterRoute("agregarReporte", typeof(AgregarReporteView));
            Routing.RegisterRoute("misReportes", typeof(MisReportesView));
            Routing.RegisterRoute("perfil", typeof(PerfilView));
            Routing.RegisterRoute("reporte", typeof(ReporteView));
            Routing.RegisterRoute("reporteAdmin", typeof(ReporteAdminView));
        }
    }
}
