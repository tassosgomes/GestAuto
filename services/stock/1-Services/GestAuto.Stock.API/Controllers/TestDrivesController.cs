using GestAuto.Stock.API.Services;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Commands;
using GestAuto.Stock.Application.TestDrives.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAuto.Stock.API.Controllers;

[ApiController]
public sealed class TestDrivesController : ControllerBase
{
    private readonly ICommandHandler<CompleteTestDriveCommand, CompleteTestDriveResponse> _complete;

    public TestDrivesController(ICommandHandler<CompleteTestDriveCommand, CompleteTestDriveResponse> complete)
    {
        _complete = complete;
    }

    [HttpPost("test-drives/{testDriveId:guid}/complete")]
    [Authorize(Policy = "SalesPerson")]
    [ProducesResponseType(typeof(CompleteTestDriveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Complete(
        [FromRoute] Guid testDriveId,
        [FromBody] CompleteTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _complete.HandleAsync(new CompleteTestDriveCommand(testDriveId, userId, request), cancellationToken);
        return Ok(result);
    }
}
