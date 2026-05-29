using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportesLocalidadApi.Models.DTOs;
using ReportesLocalidadApi.Services;
using System.Security.Claims;

namespace ReportesLocalidadApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        this.usuarioService = usuarioService;
    }

    private int GetIdUsuarioToken()
    {
        var idUsuarioTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idUsuarioTexto, out var idUsuario) ? idUsuario : 0;
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        var respuesta = await usuarioService.LogoutAsync(logoutDto);

        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }

    [HttpPost("guardarTokenFirebase")]
    public async Task<IActionResult> GuardarTokenFirebase([FromBody] TokenFirebaseDto tokenFirebaseDto)
    {
        var respuesta = await usuarioService.GuardarTokenFirebaseAsync(GetIdUsuarioToken(), tokenFirebaseDto);

        if (!respuesta.Success)
        {
            return BadRequest(respuesta);
        }

        return Ok(respuesta);
    }
}
