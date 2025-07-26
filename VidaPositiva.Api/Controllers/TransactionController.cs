using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.QueryParams.Transaction;
using VidaPositiva.Api.Services.TransactionService;
using VidaPositiva.Api.Services.UserService;

namespace VidaPositiva.Api.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class TransactionController(ITransactionService service, IUserService userService) : ControllerBase
{
    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPaginated([FromQuery] TransactionGetAllQueryParams queryParams, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var results = await service.List(user.Value.Id, queryParams, cancellationToken);
        return Ok(results);
    }
    
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] TransactionCreationInputDto input, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var result = await service.Create(input, user.Value.Id, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            r => new ObjectResult(new { Id = r }) 
            { 
                StatusCode = StatusCodes.Status201Created 
            }
        );
    }
    
    [HttpPost("bulk-create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BulkCreate([FromBody] TransactionCreationInputDto[] input, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();
        
        var result = await service.BulkCreate(input, user.Value.Id, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            r => new ObjectResult(new { Id = r }) 
            { 
                StatusCode = StatusCodes.Status201Created 
            }
        );
    }

    [HttpPost("process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessTransactionFiles([FromForm] TransactionProcessFilesInputDto form, CancellationToken cancellationToken)
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var user = await userService.GetUserByEmail(email, cancellationToken);

        if (user.IsNone)
            return Unauthorized();

        var result = await service.Process(form.Files, form.ConnectionId, user.Value.Id, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            r => r.partiallyCategorized ?
                new ObjectResult(r.transactions) 
                { 
                    StatusCode = 207 
                }
        : Ok(r.transactions)
            );
    }
}