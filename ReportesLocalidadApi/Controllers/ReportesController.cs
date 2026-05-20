using Microsoft.AspNetCore.Mvc;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Services;
using ReportesLocalidadApi.Validators;

namespace ReportesLocalidadApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReporteService _reporteService;

    public ReportesController(ReporteService reporteService)
    {
        _reporteService = reporteService;
    }


    [HttpPost("crearReporte")]
    public async Task<IActionResult> Crear([FromBody] SubirReporteDto subirReporteDto)
    {
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


    [HttpGet("obtenerReporte/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var respuesta = await _reporteService.GetByIdAsync(id);

        if (!respuesta.Success)
        {
            return NotFound(respuesta);
        }

        return Ok(respuesta);
    }


    [HttpPut("editarReporte/{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarReporteDto editarReporteDto)
    {
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


    [HttpGet("getByUsuario/{idUsuario}/")]
    public async Task<IActionResult> GetByUsuario(int idUsuario, [FromQuery] int cantidad = 25)
    {
        var respuesta = await _reporteService.GetByUsuarioAsync(idUsuario, cantidad);

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
}
