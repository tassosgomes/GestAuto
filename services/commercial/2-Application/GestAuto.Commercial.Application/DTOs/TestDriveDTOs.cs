namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// Requisição para agendar um test-drive
/// </summary>
public record ScheduleTestDriveRequest(
    /// <summary>ID do lead que fará o test-drive</summary>
    Guid LeadId,
    /// <summary>ID do veículo para test-drive</summary>
    Guid VehicleId,
    /// <summary>Data e hora agendada para o test-drive</summary>
    DateTime ScheduledAt,
    /// <summary>Observações adicionais sobre o agendamento</summary>
    string? Notes
);

/// <summary>
/// Requisição para completar um test-drive
/// </summary>
public record CompleteTestDriveRequest(
    /// <summary>Checklist de inspeção do veículo</summary>
    TestDriveChecklistDto Checklist,
    /// <summary>Feedback do cliente sobre o test-drive</summary>
    string? CustomerFeedback
);

/// <summary>
/// Requisição para cancelar um test-drive agendado
/// </summary>
public record CancelTestDriveRequest(
    /// <summary>Motivo do cancelamento</summary>
    string Reason
);

/// <summary>
/// Dados do checklist de inspeção do test-drive
/// </summary>
public record TestDriveChecklistDto(
    /// <summary>Quilometragem inicial do veículo</summary>
    decimal InitialMileage,
    /// <summary>Quilometragem final após o test-drive</summary>
    decimal FinalMileage,
    /// <summary>Nível de combustível (Vazio, Baixo, Médio, Alto, Cheio)</summary>
    string FuelLevel,
    /// <summary>Observações visuais sobre o estado do veículo</summary>
    string? VisualObservations
);

/// <summary>
/// Resposta com informações completas do test-drive
/// </summary>
public record TestDriveResponse(
    /// <summary>ID único do test-drive</summary>
    Guid Id,
    /// <summary>ID do lead</summary>
    Guid LeadId,
    /// <summary>ID do veículo</summary>
    Guid VehicleId,
    /// <summary>Status do test-drive (Scheduled, Completed, Cancelled, NoShow)</summary>
    string Status,
    /// <summary>Data e hora agendada</summary>
    DateTime ScheduledAt,
    /// <summary>Data e hora de conclusão</summary>
    DateTime? CompletedAt,
    /// <summary>ID do vendedor responsável</summary>
    Guid SalesPersonId,
    /// <summary>Observações do agendamento</summary>
    string? Notes,
    /// <summary>Checklist de inspeção (se completado)</summary>
    TestDriveChecklistResponse? Checklist,
    /// <summary>Feedback do cliente</summary>
    string? CustomerFeedback,
    /// <summary>Motivo do cancelamento (se cancelado)</summary>
    string? CancellationReason,
    /// <summary>Data de criação do agendamento</summary>
    DateTime CreatedAt,
    /// <summary>Data da última atualização</summary>
    DateTime UpdatedAt
)
{
    public static TestDriveResponse FromEntity(Domain.Entities.TestDrive testDrive) => new(
        testDrive.Id,
        testDrive.LeadId,
        testDrive.VehicleId,
        testDrive.Status.ToString(),
        testDrive.ScheduledAt,
        testDrive.CompletedAt,
        testDrive.SalesPersonId,
        testDrive.Notes,
        testDrive.Checklist != null 
            ? TestDriveChecklistResponse.FromEntity(testDrive.Checklist) 
            : null,
        testDrive.CustomerFeedback,
        testDrive.CancellationReason,
        testDrive.CreatedAt,
        testDrive.UpdatedAt
    );
}

/// <summary>
/// Resposta com dados do checklist de test-drive
/// </summary>
public record TestDriveChecklistResponse(
    /// <summary>Quilometragem inicial</summary>
    decimal InitialMileage,
    /// <summary>Quilometragem final</summary>
    decimal FinalMileage,
    /// <summary>Nível de combustível</summary>
    string FuelLevel,
    /// <summary>Observações visuais</summary>
    string? VisualObservations
)
{
    public static TestDriveChecklistResponse FromEntity(Domain.ValueObjects.TestDriveChecklist checklist) => new(
        checklist.InitialMileage,
        checklist.FinalMileage,
        checklist.FuelLevel.ToString(),
        checklist.VisualObservations
    );
}

/// <summary>
/// Item de test-drive em listagem paginada
/// </summary>
public record TestDriveListItemResponse(
    /// <summary>ID único do test-drive</summary>
    Guid Id,
    /// <summary>ID do lead</summary>
    Guid LeadId,
    /// <summary>Nome do cliente</summary>
    string LeadName,
    /// <summary>Status do test-drive</summary>
    string Status,
    /// <summary>Data e hora agendada</summary>
    DateTime ScheduledAt,
    /// <summary>Descrição do veículo (modelo e ano)</summary>
    string VehicleDescription
)
{
    public static TestDriveListItemResponse FromEntity(Domain.Entities.TestDrive testDrive, string leadName, string vehicleDescription) => new(
        testDrive.Id,
        testDrive.LeadId,
        leadName,
        testDrive.Status.ToString(),
        testDrive.ScheduledAt,
        vehicleDescription
    );
}
