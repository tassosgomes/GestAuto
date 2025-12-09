using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.Application.Handlers;

public class GetTestDriveHandler : IQueryHandler<Queries.GetTestDriveQuery, DTOs.TestDriveResponse>
{
    private readonly ITestDriveRepository _testDriveRepository;

    public GetTestDriveHandler(ITestDriveRepository testDriveRepository)
    {
        _testDriveRepository = testDriveRepository;
    }

    public async Task<DTOs.TestDriveResponse> HandleAsync(
        Queries.GetTestDriveQuery query,
        CancellationToken cancellationToken)
    {
        var testDrive = await _testDriveRepository.GetByIdAsync(query.TestDriveId, cancellationToken)
            ?? throw new NotFoundException($"Test-drive {query.TestDriveId} not found");

        return DTOs.TestDriveResponse.FromEntity(testDrive);
    }
}

public class ListTestDrivesHandler : IQueryHandler<Queries.ListTestDrivesQuery, DTOs.PagedResponse<DTOs.TestDriveListItemResponse>>
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly ILeadRepository _leadRepository;

    public ListTestDrivesHandler(ITestDriveRepository testDriveRepository, ILeadRepository leadRepository)
    {
        _testDriveRepository = testDriveRepository;
        _leadRepository = leadRepository;
    }

    public async Task<DTOs.PagedResponse<DTOs.TestDriveListItemResponse>> HandleAsync(
        Queries.ListTestDrivesQuery query,
        CancellationToken cancellationToken)
    {
        // Get total count
        var totalCount = await _testDriveRepository.CountAsync(
            query.SalesPersonId,
            query.LeadId,
            query.Status,
            query.FromDate,
            query.ToDate,
            cancellationToken);

        // Get paginated test drives
        var testDrives = await _testDriveRepository.ListAsync(
            query.SalesPersonId,
            query.LeadId,
            query.Status,
            query.FromDate,
            query.ToDate,
            query.Page,
            query.PageSize,
            cancellationToken);

        // Build list items
        var items = new List<DTOs.TestDriveListItemResponse>();
        foreach (var testDrive in testDrives)
        {
            var lead = await _leadRepository.GetByIdAsync(testDrive.LeadId, cancellationToken);
            if (lead != null)
            {
                items.Add(DTOs.TestDriveListItemResponse.FromEntity(
                    testDrive,
                    lead.Name,
                    $"Vehicle {testDrive.VehicleId}"
                ));
            }
        }

        return new DTOs.PagedResponse<DTOs.TestDriveListItemResponse>(
            items.AsReadOnly(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
