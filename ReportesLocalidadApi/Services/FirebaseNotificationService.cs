using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace ReportesLocalidadApi.Services;

public class FirebaseNotificationService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<FirebaseNotificationService> logger;
    private bool firebaseInicializado;

    public FirebaseNotificationService(IConfiguration configuration, ILogger<FirebaseNotificationService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task EnviarNotificacionAsync(string token, string titulo, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        if (!InicializarFirebase())
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
            var respuesta = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            logger.LogInformation("Notificacion Firebase enviada correctamente. Id: {Respuesta}", respuesta);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo enviar la notificación Firebase.");
        }
    }

    private bool InicializarFirebase()
    {
        if (firebaseInicializado || FirebaseApp.DefaultInstance != null)
        {
            firebaseInicializado = true;
            return true;
        }

        try
        {
            var rutaArchivo = configuration["Firebase:ServiceAccountPath"];

            if (string.IsNullOrWhiteSpace(rutaArchivo) || !File.Exists(rutaArchivo))
            {
                logger.LogWarning("No se encontró el archivo de Firebase en la ruta {RutaArchivo}.", rutaArchivo);
                return false;
            }

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(rutaArchivo)
            });

            firebaseInicializado = true;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo inicializar Firebase.");
            return false;
        }
    }
}
