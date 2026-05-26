using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Services;

namespace ReportesLocalidadApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        var respuesta = await _usuarioService.LogoutAsync(logoutDto);

        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }
}
