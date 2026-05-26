using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ReportesLocalidadApi.Models.Entities;

namespace ReportesLocalidadApi.Services;

public class JwtService
{
    private readonly IConfiguration configuration;

    public JwtService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GenerarAccessToken(Usuarios usuario)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("No se encontro la llave JWT.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var minutosTexto = configuration["Jwt:AccessTokenMinutes"];
        var minutos = int.TryParse(minutosTexto, out var valorMinutos) ? valorMinutos : 30;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim("IdRol", usuario.IdRol.ToString()),
            new Claim(ClaimTypes.Role, ObtenerNombreRol(usuario.IdRol))
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(minutos),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public DateTime ObtenerFechaExpiracionRefreshToken()
    {
        var diasTexto = configuration["Jwt:RefreshTokenDays"];
        var dias = int.TryParse(diasTexto, out var valorDias) ? valorDias : 7;
        return DateTime.Now.AddDays(dias);
    }

    private string ObtenerNombreRol(int idRol)
    {
        if (idRol == 2)
        {
            return "Admin";
        }

        return "Ciudadano";
    }
}
