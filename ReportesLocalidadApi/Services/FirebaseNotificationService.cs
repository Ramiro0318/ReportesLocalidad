using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace ReportesLocalidadApi.Services;

public class FirebaseNotificationService
{
    private readonly IConfiguration configuration;

    public FirebaseNotificationService(IConfiguration configuration)
    {
        this.configuration = configuration;
        InicializarFirebase();
    }

    public async Task EnviarNotificacionAsync(string token, string titulo, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        var message = new Message
        {
            Token = token,
            Notification = new Notification
            {
                Title = titulo,
                Body = mensaje
            }
        };

        try
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch
        {
        }
    }

    private void InicializarFirebase()
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        var rutaArchivo = configuration["Firebase:ServiceAccountPath"];

        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            throw new InvalidOperationException("No se encontro la ruta de Firebase.");
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(rutaArchivo)
        });
    }
}
