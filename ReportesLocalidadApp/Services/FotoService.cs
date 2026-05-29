using ReportesLocalidadApp.Helpers;

namespace ReportesLocalidadApp.Services;

public class FotoService
{
    private int MaxSizeBytes = 8 * 1024 * 1024;
    private readonly FotoHelper fotoHelper;

    public FotoService(FotoHelper fotoHelper)
    {
        this.fotoHelper = fotoHelper;
    }

    public async Task<(string? RutaImagen, string? FotoBase64, string? Error)> TomarFotoAsync()
    {
        try
        {
            var permiso = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permiso != PermissionStatus.Granted)
            {
                permiso = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (permiso != PermissionStatus.Granted)
            {
                return (null, null, $"No se concedió permiso para usar la cámara. Estado: {permiso}");
            }

            if (!MediaPicker.Default.IsCaptureSupported)
            {   //No sé si sea posible darse este caso, pero como ya funciona, lo dejo por si las dudas
                return (null, null, "El dispositivo no permite tomar fotografías.");
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo == null)  //Validacion local
            {
                return (null, null, "No se tomo ninguna fotografía.");
            }

            return await GuardarImagenAsync(photo);
        }
        catch (FeatureNotSupportedException)
        {
            return (null, null, "La cámara no esta disponible en este dispositivo.");
        }
        catch
        {
            return (null, null, "No se pudo abrir la cámara.");
        }
    }

    public async Task<(string? RutaImagen, string? FotoBase64, string? Error)> SeleccionarFotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo is null)
            {
                return (null, null, "No se seleccionó ninguna fotografía.");
            }

            return await GuardarImagenAsync(photo);
        }
        catch (FeatureNotSupportedException)
        {
            return (null, null, "La galería no esta disponible en este dispositivo.");
        }
        catch (PermissionException)
        {
            return (null, null, "No se concedió permiso para abrir la galería.");
        }
        catch
        {
            return (null, null, "No se pudo abrir la galería.");
        }
    }

    private async Task<(string? RutaImagen, string? FotoBase64, string? Error)> GuardarImagenAsync(FileResult photo)
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
                return (null, null, "Formato de imagen no permitido.");
            }

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var localFilePath = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            await using (var sourceStream = await photo.OpenReadAsync())        //sirve para liberar correctamente un recurso asíncrono cuando terminas de usarlo.
            await using (var localFileStream = File.OpenWrite(localFilePath))
            {
                await sourceStream.CopyToAsync(localFileStream);
            }

            var rutaComprimida = await fotoHelper.ComprimirFotoAsync(localFilePath);
            var buffer = await File.ReadAllBytesAsync(rutaComprimida);

            if (buffer.Length > MaxSizeBytes)
            {
                return (null, null, "La imagen excede el tamaño maximo permitido.");
            }

            //var base64 = $"{ObtenerPrefijoBase64(Path.GetExtension(rutaComprimida).ToLower())}{Convert.ToBase64String(buffer)}";

            var extensionComprimida = Path.GetExtension(rutaComprimida).ToLower();
            var prefijoBase64 = "data:image/jpeg;base64,";

            if (extensionComprimida == ".png")
            {
                prefijoBase64 = "data:image/png;base64,";
            }

            var base64 = $"{prefijoBase64}{Convert.ToBase64String(buffer)}";

            return (rutaComprimida, base64, null);
        }
        catch
        {
            return (null, null, "No se pudo procesar la fotografía.");
        }
    }

    //private string ObtenerPrefijoBase64(string extension)
    //{
    //    if (extension == ".png")
    //    {
    //        return "data:image/png;base64,";
    //    }

    //    return "data:image/jpeg;base64,";
    //}
}
