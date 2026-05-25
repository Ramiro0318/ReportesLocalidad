namespace ReportesLocalidadApp.Services;

public class FotoService
{
    private const int MaxSizeBytes = 8 * 1024 * 1024;

    public async Task<(string RutaImagen, string FotoBase64)?> TomarFotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                return null;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo is null)
            {
                return null;
            }

            return await GuardarImagenAsync(photo);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(string RutaImagen, string FotoBase64)?> SeleccionarFotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo is null)
            {
                return null;
            }

            return await GuardarImagenAsync(photo);
        }
        catch
        {
            return null;
        }
    }

    private async Task<(string RutaImagen, string FotoBase64)?> GuardarImagenAsync(FileResult photo)
    {
        try
        {
            var extension = Path.GetExtension(photo.FileName).ToLower();

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".jpg";
            }

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return null;
            }

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var localFilePath = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            await using (var sourceStream = await photo.OpenReadAsync())
            await using (var localFileStream = File.OpenWrite(localFilePath))
            {
                await sourceStream.CopyToAsync(localFileStream);
            }

            var buffer = await File.ReadAllBytesAsync(localFilePath);

            if (buffer.Length > MaxSizeBytes)
            {
                return null;
            }

            var base64 = $"{ObtenerPrefijoBase64(extension)}{Convert.ToBase64String(buffer)}";

            return (localFilePath, base64);
        }
        catch
        {
            return null;
        }
    }

    private string ObtenerPrefijoBase64(string extension)
    {
        if (extension == ".png")
        {
            return "data:image/png;base64,";
        }

        return "data:image/jpeg;base64,";
    }
}
