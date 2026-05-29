namespace ReportesLocalidadApp.Services;

public class FirebaseTokenService
{
    //no se si debe ser un service separado
    private string FirebaseTokenKey = "firebase_token";

    public async Task GuardarTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(FirebaseTokenKey, token);
        System.Diagnostics.Debug.WriteLine($"TOKEN FCM: {token}");
    }

    public async Task<string?> ObtenerTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(FirebaseTokenKey);
    }
}
