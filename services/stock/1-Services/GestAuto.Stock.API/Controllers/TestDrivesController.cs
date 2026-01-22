using GestAuto.Stock.API.Services;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Commands;
using GestAuto.Stock.Application.TestDrives.Dto;
using GestAuto.Stock.Application.TestDrives.Queries;
using GestAuto.Stock.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAuto.Stock.API.Controllers;

[ApiController]
public sealed class TestDrivesController : ControllerBase
{
    private readonly ICommandHandler<CompleteTestDriveCommand, CompleteTestDriveResponse> _complete;
    private readonly IQueryHandler<ListTestDrivesQuery, GestAuto.Stock.Application.Common.PagedResponse<TestDriveListItem>> _list;

    public TestDrivesController(
        ICommandHandler<CompleteTestDriveCommand, CompleteTestDriveResponse> complete,
        IQueryHandler<ListTestDrivesQuery, GestAuto.Stock.Application.Common.PagedResponse<TestDriveListItem>> list)
    {
        _complete = complete;
        _list = list;
    }

    [HttpGet("test-drives")]
    [Authorize]
    [ProducesResponseType(typeof(GestAuto.Stock.Application.Common.PagedResponse<TestDriveListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery(Name = "_page")] int _page = 1,
        [FromQuery(Name = "_size")] int _size = 10,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] Guid? salesPersonId = null,
        [FromQuery] string? leadId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var effectivePage = page ?? _page;
        var effectiveSize = pageSize ?? _size;

        if (effectivePage < 1 || effectiveSize < 1)
        {
            throw new DomainException("Pagination parameters must be positive.");
        }

        var parsedStatus = ParseStatus(status);
        var customerRef = string.IsNullOrWhiteSpace(leadId) ? null : leadId.Trim();

        var result = await _list.HandleAsync(
            new ListTestDrivesQuery(
                Page: effectivePage,
                Size: effectiveSize,
                Status: parsedStatus,
                VehicleId: vehicleId,
                SalesPersonId: salesPersonId,
                CustomerRef: customerRef,
                From: from,
                To: to),
            cancellationToken);

        return Ok(result);
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

    private static TestDriveStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Equals("InProgress", StringComparison.OrdinalIgnoreCase))
        {
            return TestDriveStatus.Scheduled;
        }

        if (Enum.TryParse<TestDriveStatus>(normalized, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new DomainException($"Invalid value for TestDriveStatus: {value}");
    }
}
