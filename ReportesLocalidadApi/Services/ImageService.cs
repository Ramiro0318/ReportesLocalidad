namespace ReportesLocalidadApi.Services;

public class ImageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string[] _formatosPermitidos = [".jpg", ".jpeg", ".png"];
    private const int MaxSizeBytes = 2 * 1024 * 1024;

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> GuardarImagenBase64Async(string? imagenBase64)
    {
        if (string.IsNullOrWhiteSpace(imagenBase64))
        {
            return null;
        }

        var extension = ObtenerExtension(imagenBase64);
        if (!_formatosPermitidos.Contains(extension))
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

        if (bytesImagen.Length > MaxSizeBytes)
        {
            throw new InvalidOperationException("La imagen excede el tamano maximo permitido.");
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "reportes");
        Directory.CreateDirectory(uploadsPath);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaFisica = Path.Combine(uploadsPath, nombreArchivo);

        await File.WriteAllBytesAsync(rutaFisica, bytesImagen);

        return $"/uploads/reportes/{nombreArchivo}";
    }

    private static string LimpiarBase64(string imagenBase64)
    {
        var indiceComa = imagenBase64.IndexOf(',');
        return indiceComa >= 0
            ? imagenBase64[(indiceComa + 1)..]
            : imagenBase64;
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
