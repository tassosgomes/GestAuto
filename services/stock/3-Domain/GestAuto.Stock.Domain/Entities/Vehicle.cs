using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Events;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.History;

namespace GestAuto.Stock.Domain.Entities;

public sealed class Vehicle : BaseEntity
{
    private readonly List<CheckInRecord> _checkIns = new();
    private readonly List<CheckOutRecord> _checkOuts = new();
    private readonly List<TestDriveSession> _testDrives = new();

    public VehicleCategory Category { get; private set; }
    public VehicleStatus CurrentStatus { get; private set; }

    public string Vin { get; private set; } = null!;
    public string? Plate { get; private set; }

    public string Make { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string? Trim { get; private set; }
    public int YearModel { get; private set; }
    public string Color { get; private set; } = null!;

    public int? MileageKm { get; private set; }
    public Guid? EvaluationId { get; private set; }

    public DemoPurpose? DemoPurpose { get; private set; }
    public bool IsRegistered { get; private set; }

    public Guid? CurrentOwnerUserId { get; private set; }

    public IReadOnlyCollection<CheckInRecord> CheckIns => _checkIns.AsReadOnly();
    public IReadOnlyCollection<CheckOutRecord> CheckOuts => _checkOuts.AsReadOnly();
    public IReadOnlyCollection<TestDriveSession> TestDrives => _testDrives.AsReadOnly();

    private Vehicle() { }

    public Vehicle(
        VehicleCategory category,
        string vin,
        string make,
        string model,
        int yearModel,
        string color,
        string? plate = null,
        string? trim = null,
        int? mileageKm = null,
        Guid? evaluationId = null,
        DemoPurpose? demoPurpose = null,
        bool isRegistered = false)
    {
        Category = category;
        Vin = RequireNonEmpty(vin, nameof(vin));
        Make = RequireNonEmpty(make, nameof(make));
        Model = RequireNonEmpty(model, nameof(model));
        YearModel = yearModel;
        Color = RequireNonEmpty(color, nameof(color));

        Plate = plate;
        Trim = trim;
        MileageKm = mileageKm;
        EvaluationId = evaluationId;
        DemoPurpose = demoPurpose;
        IsRegistered = isRegistered;

        CurrentStatus = VehicleStatus.InTransit;
    }

    public void UpdateIdentifiers(string? plate)
    {
        Plate = plate;
        Touch();
    }

    public void UpdateUsedInfo(int? mileageKm, Guid? evaluationId)
    {
        MileageKm = mileageKm;
        EvaluationId = evaluationId;
        Touch();
    }

    public void UpdateDemoInfo(DemoPurpose? demoPurpose, bool isRegistered)
    {
        DemoPurpose = demoPurpose;
        IsRegistered = isRegistered;
        Touch();
    }

    public void CheckIn(CheckInSource source, DateTime occurredAt, Guid responsibleUserId, string? notes = null)
    {
        ValidateCategoryRequirementsForCheckIn(source);

        var record = new CheckInRecord(Id, source, occurredAt, responsibleUserId, notes);
        _checkIns.Add(record);

        CurrentOwnerUserId = responsibleUserId;

        var newStatus = DetermineStatusAfterCheckIn(source);
        ChangeStatus(newStatus, responsibleUserId, reason: $"check-in:{source}");

        AddEvent(new VehicleCheckedInEvent(Id, source, occurredAt, responsibleUserId));

        Touch();
    }

    public void MarkInStock(Guid changedByUserId, string? reason = null)
    {
        EnsureTransitionAllowed(VehicleStatus.InStock);
        ChangeStatus(VehicleStatus.InStock, changedByUserId, reason ?? "marked-in-stock");
    }

    public void MarkInPreparation(Guid changedByUserId, string? reason = null)
    {
        EnsureTransitionAllowed(VehicleStatus.InPreparation);
        ChangeStatus(VehicleStatus.InPreparation, changedByUserId, reason ?? "marked-in-preparation");
    }

    public void ChangeStatusManually(VehicleStatus newStatus, Guid changedByUserId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Reason is required.");
        }

        EnsureTransitionAllowed(newStatus);
        ChangeStatus(newStatus, changedByUserId, $"manual:{reason.Trim()}");
    }

    public void Reserve(Guid reservationId, Guid salesPersonId)
    {
        if (CurrentStatus is VehicleStatus.Sold or VehicleStatus.WrittenOff)
        {
            throw new DomainException("Cannot reserve a sold or written-off vehicle.");
        }

        if (CurrentStatus is not VehicleStatus.InStock)
        {
            throw new DomainException("Vehicle must be in stock to be reserved.");
        }

        ChangeStatus(VehicleStatus.Reserved, salesPersonId, "reservation-created");

        AddEvent(new ReservationCreatedEvent(reservationId, Id, salesPersonId));
        Touch();
    }

    public void ReleaseReservation(Guid reservationId, Guid releasedByUserId, string reason)
    {
        if (CurrentStatus is not VehicleStatus.Reserved)
        {
            throw new DomainException("Vehicle is not reserved.");
        }

        ChangeStatus(VehicleStatus.InStock, releasedByUserId, reason);
        AddEvent(new ReservationReleasedEvent(reservationId, Id, releasedByUserId, reason));
        Touch();
    }

    public Guid StartTestDrive(Guid salesPersonId, string? customerRef, DateTime startedAt)
    {
        if (CurrentStatus is VehicleStatus.Sold or VehicleStatus.WrittenOff)
        {
            throw new DomainException("Cannot start test-drive for a sold or written-off vehicle.");
        }

        if (CurrentStatus is not (VehicleStatus.InStock or VehicleStatus.Reserved))
        {
            throw new DomainException("Vehicle must be in stock or reserved to start test-drive.");
        }

        EnsureTransitionAllowed(VehicleStatus.InTestDrive);

        var session = new TestDriveSession(Id, salesPersonId, customerRef, startedAt);
        _testDrives.Add(session);

        ChangeStatus(VehicleStatus.InTestDrive, salesPersonId, "test-drive-started");
        AddEvent(new VehicleTestDriveStartedEvent(session.Id, Id, salesPersonId, startedAt));

        Touch();
        return session.Id;
    }

    public void CompleteTestDrive(Guid testDriveId, Guid completedByUserId, DateTime endedAt, TestDriveOutcome outcome)
    {
        var session = _testDrives.SingleOrDefault(s => s.Id == testDriveId);
        if (session is null)
        {
            throw new NotFoundException("Test-drive session not found.");
        }

        if (CurrentStatus is not VehicleStatus.InTestDrive)
        {
            throw new DomainException("Vehicle is not in test-drive status.");
        }

        session.Complete(endedAt, outcome);

        var nextStatus = outcome switch
        {
            TestDriveOutcome.ReturnedToStock => VehicleStatus.InStock,
            TestDriveOutcome.ConvertedToReservation => VehicleStatus.Reserved,
            _ => throw new DomainException("Invalid test-drive outcome.")
        };

        EnsureTransitionAllowed(nextStatus);
        ChangeStatus(nextStatus, completedByUserId, $"test-drive-completed:{outcome}");
        AddEvent(new VehicleTestDriveCompletedEvent(testDriveId, Id, completedByUserId, endedAt, outcome));

        Touch();
    }

    public void CheckOut(CheckOutReason reason, DateTime occurredAt, Guid responsibleUserId, string? notes = null)
    {
        if (CurrentStatus is VehicleStatus.Sold or VehicleStatus.WrittenOff)
        {
            throw new DomainException("Vehicle is already finalized (sold or written-off)." );
        }

        var record = new CheckOutRecord(Id, reason, occurredAt, responsibleUserId, notes);
        _checkOuts.Add(record);

        switch (reason)
        {
            case CheckOutReason.Sale:
                EnsureTransitionAllowed(VehicleStatus.Sold);
                ChangeStatus(VehicleStatus.Sold, responsibleUserId, "check-out:sale");
                AddEvent(new VehicleSoldEvent(Id, occurredAt, responsibleUserId));
                break;

            case CheckOutReason.TotalLoss:
                EnsureTransitionAllowed(VehicleStatus.WrittenOff);
                ChangeStatus(VehicleStatus.WrittenOff, responsibleUserId, "check-out:total-loss");
                AddEvent(new VehicleWrittenOffEvent(Id, occurredAt, responsibleUserId));
                break;

            case CheckOutReason.TestDrive:
                EnsureTransitionAllowed(VehicleStatus.InTestDrive);
                ChangeStatus(VehicleStatus.InTestDrive, responsibleUserId, "check-out:test-drive");
                break;

            case CheckOutReason.Transfer:
                EnsureTransitionAllowed(VehicleStatus.InTransit);
                ChangeStatus(VehicleStatus.InTransit, responsibleUserId, "check-out:transfer");
                break;

            default:
                throw new DomainException("Invalid check-out reason.");
        }

        Touch();
    }

    private void ValidateCategoryRequirementsForCheckIn(CheckInSource source)
    {
        if (string.IsNullOrWhiteSpace(Vin))
        {
            throw new DomainException("VIN is required.");
        }

        switch (Category)
        {
            case VehicleCategory.New:
                if (source != CheckInSource.Manufacturer)
                {
                    throw new DomainException("New vehicles must be checked-in with Manufacturer source.");
                }

                break;

            case VehicleCategory.Used:
                if (string.IsNullOrWhiteSpace(Plate))
                {
                    throw new DomainException("Plate is required for used vehicles.");
                }

                if (!MileageKm.HasValue || MileageKm.Value < 0)
                {
                    throw new DomainException("MileageKm is required for used vehicles.");
                }

                if (!EvaluationId.HasValue || EvaluationId.Value == Guid.Empty)
                {
                    throw new DomainException("EvaluationId is required for used vehicles.");
                }

                break;

            case VehicleCategory.Demonstration:
                if (!DemoPurpose.HasValue)
                {
                    throw new DomainException("DemoPurpose is required for demonstration vehicles.");
                }

                if (IsRegistered && string.IsNullOrWhiteSpace(Plate))
                {
                    throw new DomainException("Plate is required when the demonstration vehicle is registered.");
                }

                break;

            default:
                throw new DomainException("Invalid vehicle category.");
        }
    }

    private VehicleStatus DetermineStatusAfterCheckIn(CheckInSource source)
    {
        return Category switch
        {
            VehicleCategory.New => VehicleStatus.InTransit,
            VehicleCategory.Used => VehicleStatus.InStock,
            VehicleCategory.Demonstration => VehicleStatus.InStock,
            _ => throw new DomainException("Invalid vehicle category.")
        };
    }

    private void EnsureTransitionAllowed(VehicleStatus newStatus)
    {
        if (!IsTransitionAllowed(CurrentStatus, newStatus))
        {
            throw new DomainException($"Invalid status transition: {CurrentStatus} -> {newStatus}.");
        }
    }

    private void ChangeStatus(VehicleStatus newStatus, Guid changedByUserId, string reason)
    {
        if (newStatus == CurrentStatus)
        {
            return;
        }

        var previous = CurrentStatus;
        CurrentStatus = newStatus;

        AddEvent(new VehicleStatusChangedEvent(Id, previous, newStatus, changedByUserId, reason));
        Touch();
    }

    private static bool IsTransitionAllowed(VehicleStatus current, VehicleStatus next)
    {
        if (current == next)
        {
            return true;
        }

        return current switch
        {
            VehicleStatus.InTransit => next is VehicleStatus.InStock,
            VehicleStatus.InStock => next is VehicleStatus.Reserved or VehicleStatus.InTestDrive or VehicleStatus.InPreparation or VehicleStatus.Sold or VehicleStatus.WrittenOff or VehicleStatus.InTransit,
            VehicleStatus.Reserved => next is VehicleStatus.InStock or VehicleStatus.InTestDrive or VehicleStatus.Sold,
            VehicleStatus.InTestDrive => next is VehicleStatus.InStock or VehicleStatus.Reserved,
            VehicleStatus.InPreparation => next is VehicleStatus.InStock,
            VehicleStatus.Sold => false,
            VehicleStatus.WrittenOff => false,
            _ => false
        };
    }

    private static string RequireNonEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} is required.");
        }

        return value.Trim();
    }
}
