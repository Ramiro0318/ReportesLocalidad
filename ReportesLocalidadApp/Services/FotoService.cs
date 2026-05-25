namespace ReportesLocalidadApp.Services;

public class FotoService
{
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

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return null;
            }

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var localFilePath = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            using var sourceStream = await photo.OpenReadAsync();
            using var localFileStream = File.OpenWrite(localFilePath);
            await sourceStream.CopyToAsync(localFileStream);

            var buffer = await File.ReadAllBytesAsync(localFilePath);
            var base64 = Convert.ToBase64String(buffer);

            return (localFilePath, base64);
        }
        catch
        {
            return null;
        }
    }
}
