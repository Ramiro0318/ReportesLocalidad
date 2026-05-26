using Android.Widget;

namespace ReportesLocalidadApp.Services;

public partial class AlertaService
{
    partial void MostrarToastAndroid(string mensaje, ref bool mostrado)
    {
        mostrado = true;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var contexto = Platform.CurrentActivity ?? Android.App.Application.Context;
            Toast.MakeText(contexto, mensaje, ToastLength.Short)?.Show();
        });
    }
}
