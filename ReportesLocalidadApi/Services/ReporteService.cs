using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Models.Entities;
using ReportesLocalidadApi.Repositories;

namespace ReportesLocalidadApi.Services;

public class ReporteService
{
    private readonly ReportesLocalidadContext _context;
    private readonly Repository<Reportes> _reporteRepository;
    private readonly ImageService _imageService;
    private readonly IMapper _mapper;

    public ReporteService(ReportesLocalidadContext context, Repository<Reportes> reporteRepository, ImageService imageService, IMapper mapper)
    {
        _context = context;
        _reporteRepository = reporteRepository;
        _imageService = imageService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<Reportes>> CrearAsync(SubirReporteDto subirReporteDto)
    {
        var usuarioExiste = await _context.Usuarios
            .AnyAsync(usuario => usuario.Id == subirReporteDto.IdUsuario);

        if (!usuarioExiste)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Usuario no encontrado."
            };
        }

        var categoriaExiste = await _context.Categorias
            .AnyAsync(categoria => categoria.Id == subirReporteDto.IdCategoria);

        if (!categoriaExiste)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Categoria no encontrada."
            };
        }

        if (!string.IsNullOrWhiteSpace(subirReporteDto.ClientRequestId))
        {
            var reporteDuplicado = await _context.Reportes
                .AnyAsync(reporte => reporte.ClientRequestId == subirReporteDto.ClientRequestId);

            if (reporteDuplicado)
            {
                return new ApiResponse<Reportes>
                {
                    Success = false,
                    Message = "Este reporte ya fue registrado anteriormente."
                };
            }
        }

        var reporte = _mapper.Map<Reportes>(subirReporteDto);

        reporte.FechaSubida = DateTime.Now;
        reporte.IdEstado = 1;
        reporte.ImgUrl = await _imageService.GuardarImagenBase64Async(subirReporteDto.Foto);

        await _reporteRepository.AddAsync(reporte);
        await _reporteRepository.SaveChangesAsync();

        return new ApiResponse<Reportes>
        {
            Success = true,
            Message = "Reporte creado correctamente.",
            Data = reporte
        };
    }

    public async Task<ApiResponse<Reportes>> GetByIdAsync(int id)
    {
        var reporte = await _context.Reportes
            .AsNoTracking()
            .FirstOrDefaultAsync(reporte => reporte.Id == id);

        if (reporte is null)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Reporte no encontrado."
            };
        }

        return new ApiResponse<Reportes>
        {
            Success = true,
            Message = "Reporte encontrado.",
            Data = reporte
        };
    }

    public async Task<ApiResponse<Reportes>> EditarAsync(int id, EditarReporteDto editarReporteDto)
    {
        var reporte = await _context.Reportes
            .FirstOrDefaultAsync(reporte => reporte.Id == id);

        if (reporte is null)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Reporte no encontrado."
            };
        }

        if (reporte.IdUsuario != editarReporteDto.IdUsuario)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "No puedes editar un reporte de otro usuario."
            };
        }

        var categoriaExiste = await _context.Categorias
            .AnyAsync(categoria => categoria.Id == editarReporteDto.IdCategoria);

        if (!categoriaExiste)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Categoria no encontrada."
            };
        }

        reporte.Titulo = editarReporteDto.Titulo;
        reporte.Descripcion = editarReporteDto.Descripcion;
        reporte.IdCategoria = editarReporteDto.IdCategoria;
        reporte.FechaEdicion = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(editarReporteDto.Foto))
        {
            reporte.ImgUrl = await _imageService.GuardarImagenBase64Async(editarReporteDto.Foto);
        }

        _reporteRepository.Update(reporte);
        await _reporteRepository.SaveChangesAsync();

        return new ApiResponse<Reportes>
        {
            Success = true,
            Message = "Reporte editado correctamente.",
            Data = reporte
        };
    }

    public async Task<ApiResponse<List<ReportePropioDto>>> GetByUsuarioAsync(int idUsuario, int cantidad = 50)
    {
        cantidad = Math.Clamp(cantidad, 1, 25);

        var usuarioExiste = await _context.Usuarios
            .AnyAsync(usuario => usuario.Id == idUsuario);

        if (!usuarioExiste)
        {
            return new ApiResponse<List<ReportePropioDto>>
            {
                Success = false,
                Message = "Usuario no encontrado."
            };
        }

        var reportes = await _context.Reportes
            .AsNoTracking()
            .Where(reporte => reporte.IdUsuario == idUsuario)
            .OrderByDescending(reporte => reporte.FechaEdicion ?? reporte.FechaSubida)
            .Take(cantidad)
            .Select(reporte => new ReportePropioDto
            {
                Id = reporte.Id,
                Titulo = reporte.Titulo,
                FechaSubida = reporte.FechaSubida,
                FechaEdicion = reporte.FechaEdicion,
                IdEstado = reporte.IdEstado,
                IdCategoria = reporte.IdCategoria
            })
            .ToListAsync();

        return new ApiResponse<List<ReportePropioDto>>
        {
            Success = true,
            Message = "Reportes del usuario obtenidos correctamente.",
            Data = reportes
        };
    }

    public async Task<ApiResponse<List<ReporteGeneralDto>>> GetReportesAsync(int skip = 0, int take = 50)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 50);

        var reportes = await _context.Reportes
            .AsNoTracking()
            .OrderByDescending(reporte => reporte.FechaEdicion ?? reporte.FechaSubida)
            .Skip(skip)
            .Take(take)
            .Select(reporte => new ReporteGeneralDto
            {
                Id = reporte.Id,
                Titulo = reporte.Titulo,
                nombreUsuario = reporte.IdUsuarioNavigation.NombreUsuario,
                FechaSubida = reporte.FechaSubida,
                FechaEdicion = reporte.FechaEdicion,
                IdEstado = reporte.IdEstado,
                IdCategoria = reporte.IdCategoria
            })
            .ToListAsync();

        return new ApiResponse<List<ReporteGeneralDto>>
        {
            Success = true,
            Message = "Reportes obtenidos correctamente.",
            Data = reportes
        };
    }
}
