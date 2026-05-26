using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Services;
using ReportesLocalidadApi.Validators;

namespace ReportesLocalidadApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReporteService _reporteService;

    public ReportesController(ReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    private int GetIdUsuarioToken()
    {
        var idUsuarioTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idUsuarioTexto, out var idUsuario) ? idUsuario : 0;
    }

    private int GetIdRolToken()
    {
        var idRolTexto = User.FindFirstValue("IdRol");
        return int.TryParse(idRolTexto, out var idRol) ? idRol : 0;
    }

    [HttpPost("crearReporte")]
    public async Task<IActionResult> Crear([FromBody] SubirReporteDto subirReporteDto)
    {
        subirReporteDto.IdUsuario = GetIdUsuarioToken();

        var validator = new AgregarReporteValidator();
        var validationResult = await validator.ValidateAsync(subirReporteDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = validationResult.Errors.First().ErrorMessage
            });
        }

        var respuesta = await _reporteService.CrearAsync(subirReporteDto);

        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }


    [HttpGet("getReporte/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var respuesta = await _reporteService.GetByIdAsync(id);

        if (!respuesta.Success)
        {
            return NotFound(respuesta);
        }

        return Ok(respuesta);
    }

    [HttpGet("getReporteEditar/{id}")]
    public async Task<IActionResult> GetReporteEditar(int id)
    {
        var respuesta = await _reporteService.GetReporteEditarAsync(id);

        if (!respuesta.Success)
        {
            return NotFound(respuesta);
        }

        return Ok(respuesta);
    }


    [HttpPut("editarReporte/{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarReporteDto editarReporteDto)
    {
        editarReporteDto.IdUsuario = GetIdUsuarioToken();

        var validator = new EditarReporteValidator();
        var validationResult = await validator.ValidateAsync(editarReporteDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = validationResult.Errors.First().ErrorMessage
            });
        }

        var respuesta = await _reporteService.EditarAsync(id, editarReporteDto);

        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }

    [HttpPut("cambiarEstado/{id}")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoReporteDto cambiarestadoDto)
    {
        cambiarestadoDto.IdUsuario = GetIdUsuarioToken();

        var validator = new CambiarEstadoValidator();
        var validationResult = await validator.ValidateAsync(cambiarestadoDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = validationResult.Errors.First().ErrorMessage
            });
        }

        var respuesta = await _reporteService.CambiarEstadoAsync(id, cambiarestadoDto);

        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }


    [HttpGet("getByUsuario/{idUsuario}/")]
    public async Task<IActionResult> GetByUsuario(int idUsuario, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var idUsuarioToken = GetIdUsuarioToken();
        var idRolToken = GetIdRolToken();

        if (idUsuarioToken != idUsuario && idRolToken != 2)
        {
            return Forbid();
        }

        var respuesta = await _reporteService.GetByUsuarioAsync(idUsuario, skip, take);

        if (!respuesta.Success)
        {
            return NotFound(respuesta);
        }

        return Ok(respuesta);
    }


    [HttpGet("getReportes")]
    public async Task<IActionResult> GetReportes([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var respuesta = await _reporteService.GetReportesAsync(skip, take);

        return Ok(respuesta);
    }


    [HttpDelete("eliminarReporte/{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var idUsuario = GetIdUsuarioToken();
        var respuesta = await _reporteService.EliminarAsync(id, idUsuario);
        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }
}
