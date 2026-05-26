using Microsoft.AspNetCore.Mvc;
using ReportesLocalidadApi.Models.DTOs;

namespace ReportesLocalidadApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "API funcionando correctamente.",
            Data = "Hello"
        });
    }
}
