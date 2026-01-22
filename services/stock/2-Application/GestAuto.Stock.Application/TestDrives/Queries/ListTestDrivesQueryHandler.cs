using GestAuto.Stock.Application.Common;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.TestDrives.Queries;

public sealed class ListTestDrivesQueryHandler : IQueryHandler<ListTestDrivesQuery, PagedResponse<TestDriveListItem>>
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public ListTestDrivesQueryHandler(
        ITestDriveRepository testDriveRepository,
        IVehicleRepository vehicleRepository)
    {
        _testDriveRepository = testDriveRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<PagedResponse<TestDriveListItem>> HandleAsync(ListTestDrivesQuery query, CancellationToken cancellationToken)
    {
        var completedFilter = query.Status switch
        {
            TestDriveStatus.Completed => true,
            TestDriveStatus.Scheduled => false,
            _ => (bool?)null
        };

        var (items, total) = await _testDriveRepository.ListAsync(
            page: query.Page,
            size: query.Size,
            completed: completedFilter,
            vehicleId: query.VehicleId,
            salesPersonId: query.SalesPersonId,
            customerRef: query.CustomerRef,
            from: query.From,
            to: query.To,
            cancellationToken: cancellationToken);

        var vehicleIds = items.Select(t => t.VehicleId).Distinct().ToList();
        var vehicles = new Dictionary<Guid, Vehicle>();

        foreach (var vehicleId in vehicleIds)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
            if (vehicle is not null)
            {
                vehicles[vehicleId] = vehicle;
            }
        }

        var data = items
            .Select(testDrive =>
            {
                vehicles.TryGetValue(testDrive.VehicleId, out var vehicle);
                var description = BuildVehicleDescription(vehicle);

                return new TestDriveListItem(
                    Id: testDrive.Id,
                    LeadId: testDrive.Id,
                    LeadName: string.IsNullOrWhiteSpace(testDrive.CustomerRef) ? "Cliente não informado" : testDrive.CustomerRef,
                    Status: testDrive.EndedAt.HasValue ? "Completed" : "Scheduled",
                    ScheduledAt: testDrive.StartedAt,
                    VehicleDescription: description);
            })
            .ToList();

        var totalPages = query.Size <= 0 ? 0 : (int)Math.Ceiling(total / (double)query.Size);
        var pagination = new PaginationMetadata(query.Page, query.Size, total, totalPages);

        return new PagedResponse<TestDriveListItem>(data, pagination);
    }

    private static string BuildVehicleDescription(Vehicle? vehicle)
    {
        if (vehicle is null)
        {
            return "Veículo não encontrado";
        }

        var parts = new List<string> { vehicle.Make, vehicle.Model };
        if (!string.IsNullOrWhiteSpace(vehicle.Trim))
        {
            parts.Add(vehicle.Trim);
        }

        parts.Add(vehicle.YearModel.ToString());

        var description = string.Join(' ', parts);

        if (!string.IsNullOrWhiteSpace(vehicle.Plate))
        {
            description = $"{description} - {vehicle.Plate}";
        }

        return description;
    }
}
