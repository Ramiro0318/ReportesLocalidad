using System.Security.Cryptography;
using System.Text;

namespace ReportesLocalidadApi.Helpers;

public static class HashHelper
{
    public static string ToSha256(string textoPlano)
    {
        var bytes = Encoding.UTF8.GetBytes(textoPlano);
        var hashBytes = SHA256.HashData(bytes);

        var builder = new StringBuilder();
        foreach (var hashByte in hashBytes)
        {
            builder.Append(hashByte.ToString("x2"));
        }

        return builder.ToString();
    }

    public static bool VerifySha256(string textoPlano, string hash)
    {
        var hashTextoPlano = ToSha256(textoPlano);
        return string.Equals(hashTextoPlano, hash, StringComparison.OrdinalIgnoreCase);
    }
}
