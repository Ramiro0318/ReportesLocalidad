using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Models.Entities;
using ReportesLocalidadApi.Repositories;

namespace ReportesLocalidadApi.Services;

public class ReporteService
{
    private readonly ReportesLocalidadContext context;
    private readonly Repository<Reportes> reporteRepository;
    private readonly ImageService imageService;
    private readonly IMapper mapper;
    private readonly FirebaseNotificationService firebaseNotificationService;
    private readonly ILogger<ReporteService> logger;

    public ReporteService(ReportesLocalidadContext context, Repository<Reportes> reporteRepository, ImageService imageService, IMapper mapper, FirebaseNotificationService firebaseNotificationService, ILogger<ReporteService> logger)
    {
        this.context = context;
        this.reporteRepository = reporteRepository;
        this.imageService = imageService;
        this.mapper = mapper;
        this.firebaseNotificationService = firebaseNotificationService;
        this.logger = logger;
    }

    public async Task<ApiResponse<Reportes>> CrearAsync(SubirReporteDto subirReporteDto)
    {
        var usuarioExiste = await context.Usuarios.AnyAsync(usuario => usuario.Id == subirReporteDto.IdUsuario);

        if (!usuarioExiste)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Usuario no encontrado."
            };
        }

        var categoriaExiste = await context.Categorias.AnyAsync(categoria => categoria.Id == subirReporteDto.IdCategoria);

        if (!categoriaExiste)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Categoría no encontrada."
            };
        }

        if (!string.IsNullOrWhiteSpace(subirReporteDto.ClientRequestId))
        {
            var reporteDuplicado = await context.Reportes.AnyAsync(reporte => reporte.ClientRequestId == subirReporteDto.ClientRequestId);

            if (reporteDuplicado)
            {
                return new ApiResponse<Reportes>
                {
                    Success = false,
                    Message = "Este reporte ya fue registrado anteriormente."
                };
            }
        }

        var reporte = mapper.Map<Reportes>(subirReporteDto);

        reporte.FechaSubida = DateTime.Now;
        reporte.IdEstado = 1;

        try
        {
            reporte.ImgUrl = await imageService.GuardarImagenBase64Async(subirReporteDto.Foto);
        }
        catch (InvalidOperationException ex)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = ex.Message
            };
        }

        await reporteRepository.AddAsync(reporte);
        await reporteRepository.SaveChangesAsync();

        return new ApiResponse<Reportes>
        {
            Success = true,
            Message = "Reporte creado correctamente.",
            Data = reporte
        };
    }


    public async Task<ApiResponse<Reportes>> GetByIdAsync(int id)
    {
        var reporte = await context.Reportes.AsNoTracking().FirstOrDefaultAsync(reporte => reporte.Id == id);

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


    //public async Task<ApiResponse<ReporteAEditarDto>> GetReporteEditarAsync(int id)
    //{
    //    var reporte = await context.Reportes.AsNoTracking().Where(reporte => reporte.Id == id)
    //        .Select(reporte => new ReporteAEditarDto
    //        {
    //            Id = reporte.Id,
    //            Titulo = reporte.Titulo,
    //            Descripcion = reporte.Descripcion,
    //            Direccion = reporte.Direccion,
    //            ImgUrl = reporte.ImgUrl,
    //            IdUsuario = reporte.IdUsuario,
    //            IdCategoria = reporte.IdCategoria
    //        }).FirstOrDefaultAsync();

    //    if (reporte is null)
    //    {
    //        return new ApiResponse<ReporteAEditarDto>
    //        {
    //            Success = false,
    //            Message = "Reporte no encontrado."
    //        };
    //    }

    //    return new ApiResponse<ReporteAEditarDto>
    //    {
    //        Success = true,
    //        Message = "Reporte obtenido para edición.",
    //        Data = reporte
    //    };
    //}

    public async Task<ApiResponse<Reportes>> EditarAsync(int id, EditarReporteDto editarReporteDto)
    {
        var reporte = await context.Reportes.FirstOrDefaultAsync(reporte => reporte.Id == id);

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

        var categoriaExiste = await context.Categorias.AnyAsync(categoria => categoria.Id == editarReporteDto.IdCategoria);

        if (!categoriaExiste)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Categoría no encontrada."
            };
        }

        reporte.Titulo = editarReporteDto.Titulo;
        reporte.Descripcion = editarReporteDto.Descripcion;
        reporte.IdCategoria = editarReporteDto.IdCategoria;
        reporte.FechaEdicion = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(editarReporteDto.Foto))
        {
            try
            {
                reporte.ImgUrl = await imageService.GuardarImagenBase64Async(editarReporteDto.Foto);
            }
            catch (InvalidOperationException ex)
            {
                return new ApiResponse<Reportes>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        reporteRepository.Update(reporte);
        await reporteRepository.SaveChangesAsync();

        return new ApiResponse<Reportes>
        {
            Success = true,
            Message = "Reporte editado correctamente.",
            Data = reporte
        };
    }

    public async Task<ApiResponse<Reportes>> CambiarEstadoAsync(int id, CambiarEstadoReporteDto cambiarEstadoDto)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == cambiarEstadoDto.IdUsuario);

        if (usuario is null)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Usuario no encontrado."
            };
        }

        if (usuario.IdRol != 2)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Solo un administrador puede cambiar el estado del reporte."
            };
        }

        var reporte = await context.Reportes.FirstOrDefaultAsync(reporte => reporte.Id == id);

        if (reporte is null)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Reporte no encontrado."
            };
        }

        var estadoExiste = await context.Estados.AnyAsync(estado => estado.Id == cambiarEstadoDto.IdEstado);

        if (!estadoExiste)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Estado no encontrado."
            };
        }

        reporte.IdEstado = cambiarEstadoDto.IdEstado;
        reporte.FechaEdicion = DateTime.Now;
        var tokenFirebase = await context.Usuarios.Where(usuario => usuario.Id == reporte.IdUsuario)
            .Select(usuario => usuario.TokenFirebase)
            .FirstOrDefaultAsync();

        reporteRepository.Update(reporte);
        await reporteRepository.SaveChangesAsync();
        await NotificarCambioEstadoAsync(reporte, tokenFirebase);

        return new ApiResponse<Reportes>
        {
            Success = true,
            Message = "Estado del reporte actualizado correctamente.",
            Data = reporte
        };
    }

    private async Task NotificarCambioEstadoAsync(Reportes reporte, string? tokenFirebase)
    {
        if (string.IsNullOrWhiteSpace(tokenFirebase))
        {
            logger.LogWarning("El usuario del reporte {IdReporte} no tiene token Firebase.", reporte.Id);
            return;
        }

        var estado = ObtenerTextoEstado(reporte.IdEstado);
        var titulo = "Reporte actualizado";
        var mensaje = $"Tu reporte \"{reporte.Titulo}\" cambió a {estado}.";

        logger.LogInformation("Enviando notificacion Firebase al usuario del reporte {IdReporte}.", reporte.Id);
        await firebaseNotificationService.EnviarNotificacionAsync(tokenFirebase, titulo, mensaje);
    }

    private string ObtenerTextoEstado(int idEstado)
    {
        if (idEstado == 1) return "Pendiente";
        if (idEstado == 2) return "En progreso";
        if (idEstado == 3) return "Resuelto";
        return "un nuevo estado";
    }


    public async Task<ApiResponse<Reportes>> EliminarAsync(int id, int idUsuario)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == idUsuario);

        if (usuario is null)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Usuario no encontrado."
            };
        }

        var reporte = await context.Reportes.FirstOrDefaultAsync(reporte => reporte.Id == id);
        if (reporte is null)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "Reporte no encontrado."
            };
        }


        if (reporte.IdUsuario != idUsuario && usuario.IdRol != 2)
        {
            return new ApiResponse<Reportes>
            {
                Success = false,
                Message = "No puedes eliminar un reporte de otro usuario."
            };
        }

        reporteRepository.Delete(reporte);
        await reporteRepository.SaveChangesAsync();

        return new ApiResponse<Reportes>
        {
            Success = true,
            Message = "Reporte eliminado correctamente.",
            Data = reporte
        };
    }


    public async Task<ApiResponse<List<ReportePropioDto>>> GetByUsuarioAsync(int idUsuario, int skip = 0, int take = 50)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 50);

        var usuarioExiste = await context.Usuarios
            .AnyAsync(usuario => usuario.Id == idUsuario);

        if (!usuarioExiste)
        {
            return new ApiResponse<List<ReportePropioDto>>
            {
                Success = false,
                Message = "Usuario no encontrado."
            };
        }

        var reportes = await context.Reportes.AsNoTracking().Where(reporte => reporte.IdUsuario == idUsuario)
            .OrderByDescending(reporte => reporte.FechaEdicion ?? reporte.FechaSubida)
            .Skip(skip)
            .Take(take)
            .Select(reporte => new ReportePropioDto
            {
                Id = reporte.Id,
                Titulo = reporte.Titulo,
                FechaSubida = reporte.FechaSubida,
                FechaEdicion = reporte.FechaEdicion,
                IdEstado = reporte.IdEstado,
                IdCategoria = reporte.IdCategoria
            }).ToListAsync();

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

        var reportes = await context.Reportes.AsNoTracking()
            .OrderByDescending(reporte => reporte.FechaEdicion ?? reporte.FechaSubida)
            .Skip(skip)
            .Take(take)
            .Select(reporte => new ReporteGeneralDto
            {
                Id = reporte.Id,
                Titulo = reporte.Titulo,
                NombreUsuario = reporte.IdUsuarioNavigation.NombreUsuario,
                FechaSubida = reporte.FechaSubida,
                FechaEdicion = reporte.FechaEdicion,
                IdEstado = reporte.IdEstado,
                IdCategoria = reporte.IdCategoria
            }).ToListAsync();

        return new ApiResponse<List<ReporteGeneralDto>>
        {
            Success = true,
            Message = "Reportes obtenidos correctamente.",
            Data = reportes
        };
    }
}
