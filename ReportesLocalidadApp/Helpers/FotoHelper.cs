namespace ReportesLocalidadApp.Helpers;

public class FotoHelper
{
    private const int LadoMaximo = 1600;
    private const int CalidadInicial = 80;
    private const int CalidadMinima = 65;
    private const int PesoMaximoFinal = 2 * 1024 * 1024;

    public async Task<string> ComprimirFotoAsync(string rutaOriginal)
    {
#if ANDROID
        return await Task.Run(() => ComprimirFotoAndroid(rutaOriginal));
#else
        return rutaOriginal;
#endif
    }

#if ANDROID
    private string ComprimirFotoAndroid(string rutaOriginal)
    {
        var opciones = new Android.Graphics.BitmapFactory.Options
        {
            InJustDecodeBounds = true
        };

        Android.Graphics.BitmapFactory.DecodeFile(rutaOriginal, opciones);

        opciones.InSampleSize = CalcularEscala(opciones.OutWidth, opciones.OutHeight);
        opciones.InJustDecodeBounds = false;

        using var bitmapOriginal = Android.Graphics.BitmapFactory.DecodeFile(rutaOriginal, opciones);

        if (bitmapOriginal is null)
        {
            return rutaOriginal;
        }

        var ancho = bitmapOriginal.Width;
        var alto = bitmapOriginal.Height;
        var proporcion = Math.Min((double)LadoMaximo / ancho, (double)LadoMaximo / alto);

        Android.Graphics.Bitmap bitmapFinal;

        if (proporcion < 1)
        {
            var nuevoAncho = (int)(ancho * proporcion);
            var nuevoAlto = (int)(alto * proporcion);
            bitmapFinal = Android.Graphics.Bitmap.CreateScaledBitmap(bitmapOriginal, nuevoAncho, nuevoAlto, true);
        }
        else
        {
            bitmapFinal = bitmapOriginal;
        }

        var calidad = CalidadInicial;
        byte[] bytes;

        do
        {
            using var stream = new MemoryStream();
            bitmapFinal.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, calidad, stream);
            bytes = stream.ToArray();
            calidad -= 5;
        }
        while (bytes.Length > PesoMaximoFinal && calidad >= CalidadMinima);

        var rutaComprimida = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
        File.WriteAllBytes(rutaComprimida, bytes);

        if (!ReferenceEquals(bitmapFinal, bitmapOriginal))
        {
            bitmapFinal.Dispose();
        }

        return rutaComprimida;
    }

    private int CalcularEscala(int ancho, int alto)
    {
        var escala = 1;

        while ((alto / escala) > LadoMaximo || (ancho / escala) > LadoMaximo)
        {
            escala *= 2;
        }

        return escala;
    }
#endif
}
