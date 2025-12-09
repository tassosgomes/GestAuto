using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

public record GetTestDriveQuery(Guid TestDriveId) : IQuery<DTOs.TestDriveResponse>;

public record ListTestDrivesQuery(
    Guid? SalesPersonId,
    Guid? LeadId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
) : IQuery<DTOs.PagedResponse<DTOs.TestDriveListItemResponse>>;
