using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReportesLocalidadApi.Helpers;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Models.Entities;
using ReportesLocalidadApi.Repositories;

namespace ReportesLocalidadApi.Services;

public class UsuarioService
{
    private readonly ReportesLocalidadContext context;
    private readonly Repository<Usuarios> usuarioRepository;
    private readonly IMapper mapper;
    private readonly JwtService jwtService;

    public UsuarioService(ReportesLocalidadContext context, Repository<Usuarios> usuarioRepository, IMapper mapper, JwtService jwtService)
    {
        this.context = context;
        this.usuarioRepository = usuarioRepository;
        this.mapper = mapper;
        this.jwtService = jwtService;
    }

    public async Task<ApiResponse<UsuarioRespuestaDto>> RegistrarAsync(RegistroDto registroDto)
    {
        var usuario = mapper.Map<Usuarios>(registroDto);

        usuario.IdRol = 1;
        usuario.PasswordHash = HashHelper.ToSha256(registroDto.Password);

        await usuarioRepository.AddAsync(usuario);
        await usuarioRepository.SaveChangesAsync();

        return new ApiResponse<UsuarioRespuestaDto>
        {
            Success = true,
            Message = "Usuario registrado correctamente.",
            Data = new UsuarioRespuestaDto
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                IdRol = usuario.IdRol
            }
        };
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.NombreUsuario == loginDto.NombreUsuario);

        if (usuario is null)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Usuario o contrasena incorrectos."
            };
        }

        var passwordCorrecto = HashHelper.VerifySha256(loginDto.Password, usuario.PasswordHash);

        if (!passwordCorrecto)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Usuario o contrasena incorrectos."
            };
        }

        var refreshToken = CrearRefreshToken(usuario.Id);
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "Inicio de sesion correcto.",
            Data = new AuthResponseDto
            {
                AccessToken = jwtService.GenerarAccessToken(usuario),
                RefreshToken = refreshToken.Token,
                Usuario = CrearUsuarioRespuesta(usuario)
            }
        };
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshAsync(RefreshTokenDto refreshTokenDto)
    {
        var refreshToken = await context.RefreshTokens.Include(refreshToken => refreshToken.IdUsuarioNavigation)
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == refreshTokenDto.RefreshToken);

        if (refreshToken is null || refreshToken.FechaRevocacion != null || refreshToken.FechaExpiracion <= DateTime.Now)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Refresh token invalido o expirado."
            };
        }

        refreshToken.FechaRevocacion = DateTime.Now;

        var nuevoRefreshToken = CrearRefreshToken(refreshToken.IdUsuario);
        await context.RefreshTokens.AddAsync(nuevoRefreshToken);
        await context.SaveChangesAsync();

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "Token renovado correctamente.",
            Data = new AuthResponseDto
            {
                AccessToken = jwtService.GenerarAccessToken(refreshToken.IdUsuarioNavigation),
                RefreshToken = nuevoRefreshToken.Token,
                Usuario = CrearUsuarioRespuesta(refreshToken.IdUsuarioNavigation)
            }
        };
    }

    public async Task<ApiResponse<object>> LogoutAsync(LogoutDto logoutDto)
    {
        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(refreshToken => refreshToken.Token == logoutDto.RefreshToken);

        if (refreshToken is null)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "Refresh token no encontrado."
            };
        }

        if (refreshToken.FechaRevocacion is null)
        {
            refreshToken.FechaRevocacion = DateTime.Now;
            await context.SaveChangesAsync();
        }

        return new ApiResponse<object>
        {
            Success = true,
            Message = "Sesion cerrada correctamente."
        };
    }

    public async Task<ApiResponse<object>> GuardarTokenFirebaseAsync(int idUsuario, TokenFirebaseDto tokenFirebaseDto)
    {
        if (string.IsNullOrWhiteSpace(tokenFirebaseDto.Token))
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "Token de Firebase no valido."
            };
        }

        var usuario = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == idUsuario);

        if (usuario is null)
        {
            return new ApiResponse<object>
            {
                Success = false,
                Message = "Usuario no encontrado."
            };
        }

        usuario.TokenFirebase = tokenFirebaseDto.Token;
        await context.SaveChangesAsync();

        return new ApiResponse<object>
        {
            Success = true,
            Message = "Token de Firebase guardado correctamente."
        };
    }

    private RefreshTokens CrearRefreshToken(int idUsuario)
    {
        return new RefreshTokens
        {
            Token = jwtService.GenerarRefreshToken(),
            FechaCreacion = DateTime.Now,
            FechaExpiracion = jwtService.ObtenerFechaExpiracionToken(),
            IdUsuario = idUsuario
        };
    }

    private UsuarioRespuestaDto CrearUsuarioRespuesta(Usuarios usuario)
    {
        return new UsuarioRespuestaDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            IdRol = usuario.IdRol
        };
    }
}
