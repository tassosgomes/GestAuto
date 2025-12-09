namespace GestAuto.Commercial.Domain.Enums;

public enum OrderStatus
{
    AwaitingDocumentation = 1,
    CreditAnalysis = 2,
    CreditApproved = 3,
    CreditRejected = 4,
    AwaitingVehicle = 5,
    ReadyForDelivery = 6,
    Delivered = 7
}
