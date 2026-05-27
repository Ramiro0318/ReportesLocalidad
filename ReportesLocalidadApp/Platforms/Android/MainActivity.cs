using Android.App;
using Android.Content.PM;
using Android.Gms.Extensions;
using Android.OS;
using Firebase.Messaging;
using ReportesLocalidadApp.Services;

namespace ReportesLocalidadApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            PedirPermisoNotificaciones();
            await ObtenerTokenFirebaseAsync();
        }

        private void PedirPermisoNotificaciones()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
            {
                return;
            }

            if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                RequestPermissions(new[] { Android.Manifest.Permission.PostNotifications }, 1001);
            }
        }

        private async Task ObtenerTokenFirebaseAsync()
        {
            try
            {
                var token = await FirebaseMessaging.Instance.GetToken();
                var tokenService = IPlatformApplication.Current?.Services.GetService<FirebaseTokenService>();

                if (tokenService is not null && !string.IsNullOrWhiteSpace(token?.ToString()))
                {
                    await tokenService.GuardarTokenAsync(token.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo obtener el token FCM: {ex.Message}");
            }
        }
    }
}
