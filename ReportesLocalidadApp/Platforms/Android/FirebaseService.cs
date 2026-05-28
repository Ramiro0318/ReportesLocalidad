using Android.App;
using Firebase.Messaging;
using ReportesLocalidadApp.Services;

namespace ReportesLocalidadApp.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class FirebaseService : FirebaseMessagingService
{
    public override async void OnNewToken(string token)
    {
        base.OnNewToken(token);

        var tokenService = IPlatformApplication.Current?.Services.GetService<FirebaseTokenService>();

        if (tokenService != null)
        {
            await tokenService.GuardarTokenAsync(token);
        }

        var authService = IPlatformApplication.Current?.Services.GetService<AuthService>();

        if (authService != null)
        {
            await authService.EnviarTokenFirebaseAsync();
        }
    }
}
