using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using VidaPositiva.Api.DTOs.Inputs.Category;
using VidaPositiva.Api.OAuth.Enums;
using VidaPositiva.Api.Services.CategoryService;
using VidaPositiva.Api.Services.UserService;

namespace VidaPositiva.Api.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class CategoryController(ICategoryService service, IUserService userService) : ControllerBase
{
    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await service.GetAll();
        
        return Ok(categories);
    }

    [HttpGet("get-by-pote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParentsById([FromQuery] int poteId, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var categories = await service.GetByPoteId(poteId, user.Value.Id, cancellationToken);

        return Ok(categories);
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CategoryCreationInputDto input, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var result = await service.Create(input, user.Value.Id, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            r => Created("Categoria criada com sucesso!", r)
        );
    }
}