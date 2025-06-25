using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VidaPositiva.Api.DTOs.Inputs.Category;
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
    
    [HttpGet("get-by-id/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var result = await service.GetById(id, user.Value.Id, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            Ok);
    }
    
    [HttpGet("get-subcategory-by-id/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubCategoryById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var result = await service.GetSubCategoryById(id, user.Value.Id, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            Ok);
    }

    [HttpGet("get-by-pote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryById([FromQuery] int poteId, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var categories = await service.GetByPoteId(poteId, user.Value.Id, cancellationToken);

        return Ok(categories);
    }
    
    [HttpGet("get-by-parent-category")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubcategoryByParentId([FromQuery] int parentCategoryId, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var categories = await service.GetByParentCategoryId(parentCategoryId, user.Value.Id, cancellationToken);

        return Ok(categories);
    }
    
    [HttpPost("edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Edit([FromBody] CategoryEditInputDto input, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var result = await service.Edit(input, user.Value.Id, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            r => Ok(new { categoryId = r })
        );
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
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
            r => Created("Categoria criada com sucesso!", new { Id = r })
        );
    }
}