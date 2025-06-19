using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VidaPositiva.Api.Services.PoteService;

namespace VidaPositiva.Api.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class PoteController(IPoteService poteService) : ControllerBase
{
    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var potes = await poteService.GetAll();
        
        return Ok(potes);
    }
}