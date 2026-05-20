using Microsoft.AspNetCore.Mvc;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Services;
using ReportesLocalidadApi.Validators;

namespace ReportesLocalidadApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto registroDto)
    {
        var validator = new RegistroValidator();
        var validationResult = await validator.ValidateAsync(registroDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = validationResult.Errors.First().ErrorMessage
            });
        }

        var respuesta = await _usuarioService.RegistrarAsync(registroDto);

        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var validator = new LoginValidator();
        var validationResult = await validator.ValidateAsync(loginDto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = validationResult.Errors.First().ErrorMessage
            });
        }

        var respuesta = await _usuarioService.LoginAsync(loginDto);

        if (!respuesta.Success)
        {
            return Unauthorized(respuesta);
        }

        return Ok(respuesta);
    }
}
