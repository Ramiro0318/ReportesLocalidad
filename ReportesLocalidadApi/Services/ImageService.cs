namespace ReportesLocalidadApi.Services;

public class ImageService
{
    private IWebHostEnvironment environment;
    private List<string> formatosPermitidos = new List<string>(){ ".jpg", ".jpeg", ".png" };

    public ImageService(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    public async Task<string?> GuardarImagenBase64Async(string? imagenBase64)
    {
        if (string.IsNullOrWhiteSpace(imagenBase64))
        {
            return null;
        }

        var extension = ObtenerExtension(imagenBase64);
        if (!formatosPermitidos.Contains(extension))
        {
            throw new InvalidOperationException("Formato de imagen no permitido.");
        }

        var base64Limpio = LimpiarBase64(imagenBase64);
        byte[] bytesImagen;

        try
        {
            bytesImagen = Convert.FromBase64String(base64Limpio);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("La imagen no tiene un formato base64 valido.");
        }

        if (bytesImagen.Length > (8 * 1024 * 1024))
        {
            throw new InvalidOperationException("La imagen excede el tamaño maximo permitido.");
        }

        var webRootPath = environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var uploadsPath = Path.Combine(webRootPath, "uploads");

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaFisica = Path.Combine(uploadsPath, nombreArchivo);

        try
        {
            Directory.CreateDirectory(uploadsPath);
            await File.WriteAllBytesAsync(rutaFisica, bytesImagen);
        }
        catch
        {
            throw new InvalidOperationException("No se pudo guardar la imagen en el servidor.");
        }

        return $"/uploads/{nombreArchivo}";
    }

    private static string LimpiarBase64(string imagenBase64)
    {
        var indiceComa = imagenBase64.IndexOf(',');
        if (indiceComa >= 0)
        {
            return imagenBase64.Substring(indiceComa + 1);
        }

        return imagenBase64;
    }

    private static string ObtenerExtension(string imagenBase64)
    {
        if (imagenBase64.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase))
        {
            return ".jpg";
        }

        if (imagenBase64.StartsWith("data:image/jpg;base64,", StringComparison.OrdinalIgnoreCase))
        {
            return ".jpg";
        }

        if (imagenBase64.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
        {
            return ".png";
        }

        return ".jpg";
    }
}
