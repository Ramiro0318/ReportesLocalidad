namespace ReportesLocalidadApp.Services;

public partial class AlertaService
{
    public async Task MostrarToastAsync(string mensaje)
    {
        var mostrado = false;
        MostrarToastAndroid(mensaje, ref mostrado);

        if (mostrado)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlert("Aviso", mensaje, "Aceptar");
        });
    }

    partial void MostrarToastAndroid(string mensaje, ref bool mostrado);
}
