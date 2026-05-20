using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReportesLocalidadApi.Helpers;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Models.Entities;
using ReportesLocalidadApi.Repositories;

namespace ReportesLocalidadApi.Services;

public class UsuarioService
{
    private readonly ReportesLocalidadContext _context;
    private readonly Repository<Usuarios> _usuarioRepository;
    private readonly IMapper _mapper;

    public UsuarioService(ReportesLocalidadContext context, Repository<Usuarios> usuarioRepository, IMapper mapper)
    {
        _context = context;
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UsuarioRespuestaDto>> RegistrarAsync(RegistroDto registroDto)
    {
        var usuario = _mapper.Map<Usuarios>(registroDto);

        usuario.IdRol = 1;
        usuario.PasswordHash = HashHelper.ToSha256(registroDto.Password);

        await _usuarioRepository.AddAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

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

    public async Task<ApiResponse<UsuarioRespuestaDto>> LoginAsync(LoginDto loginDto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.NombreUsuario == loginDto.NombreUsuario);

        if (usuario is null)
        {
            return new ApiResponse<UsuarioRespuestaDto>
            {
                Success = false,
                Message = "Usuario o contrasena incorrectos."
            };
        }

        var passwordCorrecto = HashHelper.VerifySha256(loginDto.Password, usuario.PasswordHash);

        if (!passwordCorrecto)
        {
            return new ApiResponse<UsuarioRespuestaDto>
            {
                Success = false,
                Message = "Usuario o contrasena incorrectos."
            };
        }

        return new ApiResponse<UsuarioRespuestaDto>
        {
            Success = true,
            Message = "Inicio de sesion correcto.",
            Data = new UsuarioRespuestaDto
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                IdRol = usuario.IdRol
            }
        };
    }

}
