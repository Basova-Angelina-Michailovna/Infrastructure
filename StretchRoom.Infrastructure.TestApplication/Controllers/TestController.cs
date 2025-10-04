using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StretchRoom.Infrastructure.Helpers;
using StretchRoom.Infrastructure.TestApplication.Commands;

namespace StretchRoom.Infrastructure.TestApplication.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class TestController : ControllerBase
{
    [HttpGet("ok")]
    public IActionResult GetOk()
    {
        return Ok();
    }

    [HttpGet("json")]
    public IActionResult GetJson()
    {
        return Ok(new { Message = "OK" });
    }

    [HttpGet("exception")]
    public IActionResult GetException()
    {
        throw new Exception("Test");
    }

    [HttpPost("body")]
    public IActionResult PostBody([Required] [FromBody] SomeBody someBody)
    {
        return Ok(someBody);
    }

    [HttpGet("query")]
    public IActionResult GetQuery([Required] [FromQuery] int value)
    {
        return Ok(new { Message = value });
    }

    [HttpPost("command")]
    public async Task<IActionResult> ExecuteCommand(
        [Required, FromQuery] string name,
        [FromServices] IScopedCommandExecutor commandExecutor,
        CancellationToken token)
    {
        var result =
            await commandExecutor.ExecuteAsync<AddEntityCommandContext, AddEntityCommandResult>(
                new AddEntityCommandContext(name), token);
        return Ok(result);
    }

    [HttpGet("command")]
    public async Task<IActionResult> ExecuteCommand(
        [FromServices] IScopedCommandExecutor commandExecutor,
        CancellationToken token)
    {
        var result =
            await commandExecutor.ExecuteAsync<GetEntitiesCommandContext, GetEntitiesCommandResult>(
                new GetEntitiesCommandContext(), token);
        return Ok(result);
    }
}

public record SomeBody(string Message);