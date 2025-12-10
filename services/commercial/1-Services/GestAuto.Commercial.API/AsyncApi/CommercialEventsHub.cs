using Saunter.Attributes;
using GestAuto.Commercial.Domain.Events;

namespace GestAuto.Commercial.API.AsyncApi;

/// <summary>
/// Hub de eventos assíncronos do módulo comercial.
/// Esta classe documenta os eventos publicados e consumidos via RabbitMQ.
/// </summary>
[AsyncApi]
public class CommercialEventsHub
{
    // ===== EVENTOS DE LEAD =====

    /// <summary>
    /// Evento publicado quando um novo lead é criado no sistema.
    /// </summary>
    /// <remarks>
    /// Routing Key: lead.created
    /// Exchange: gestauto.commercial
    /// </remarks>
    [Channel("lead.created", Description = "Canal para notificação de criação de leads")]
    [PublishOperation(
        OperationId = "LeadCreatedPublish",
        Summary = "Lead criado",
        Description = "Publicado quando um novo lead é cadastrado no sistema. Útil para integração com CRM e marketing automation.")]
    public LeadCreatedEvent LeadCreated(LeadCreatedEvent @event) => @event;

    /// <summary>
    /// Evento publicado quando o status de um lead é alterado.
    /// </summary>
    [Channel("lead.status.changed", Description = "Canal para notificação de mudança de status de leads")]
    [PublishOperation(
        OperationId = "LeadStatusChangedPublish",
        Summary = "Status do lead alterado",
        Description = "Publicado quando o status de um lead muda (ex: Novo -> Qualificado -> Convertido). Permite tracking do funil de vendas.")]
    public LeadStatusChangedEvent LeadStatusChanged(LeadStatusChangedEvent @event) => @event;

    /// <summary>
    /// Evento publicado quando um lead recebe uma pontuação de qualificação.
    /// </summary>
    [Channel("lead.scored", Description = "Canal para notificação de scoring de leads")]
    [PublishOperation(
        OperationId = "LeadScoredPublish",
        Summary = "Lead pontuado",
        Description = "Publicado quando o sistema calcula ou atualiza o score de qualificação de um lead baseado em critérios como orçamento, urgência e interesse.")]
    public LeadScoredEvent LeadScored(LeadScoredEvent @event) => @event;

    // ===== EVENTOS DE PROPOSTA =====

    /// <summary>
    /// Evento publicado quando uma nova proposta comercial é criada.
    /// </summary>
    [Channel("proposal.created", Description = "Canal para notificação de criação de propostas")]
    [PublishOperation(
        OperationId = "ProposalCreatedPublish",
        Summary = "Proposta criada",
        Description = "Publicado quando uma nova proposta comercial é criada para um lead. Inicia o processo de negociação.")]
    public ProposalCreatedEvent ProposalCreated(ProposalCreatedEvent @event) => @event;

    /// <summary>
    /// Evento publicado quando uma proposta é atualizada.
    /// </summary>
    [Channel("proposal.updated", Description = "Canal para notificação de atualização de propostas")]
    [PublishOperation(
        OperationId = "ProposalUpdatedPublish",
        Summary = "Proposta atualizada",
        Description = "Publicado quando uma proposta é modificada (valores, descontos, condições). Permite auditoria de negociações.")]
    public ProposalUpdatedEvent ProposalUpdated(ProposalUpdatedEvent @event) => @event;

    // ===== EVENTOS DE TEST-DRIVE =====

    /// <summary>
    /// Evento publicado quando um test-drive é agendado.
    /// </summary>
    [Channel("testdrive.scheduled", Description = "Canal para notificação de agendamento de test-drives")]
    [PublishOperation(
        OperationId = "TestDriveScheduledPublish",
        Summary = "Test-drive agendado",
        Description = "Publicado quando um test-drive é agendado. Permite reserva de veículo e notificação ao cliente.")]
    public TestDriveScheduledEvent TestDriveScheduled(TestDriveScheduledEvent @event) => @event;

    /// <summary>
    /// Evento publicado quando um test-drive é concluído.
    /// </summary>
    [Channel("testdrive.completed", Description = "Canal para notificação de conclusão de test-drives")]
    [PublishOperation(
        OperationId = "TestDriveCompletedPublish",
        Summary = "Test-drive concluído",
        Description = "Publicado quando um test-drive é finalizado. Inclui feedback do cliente e resultado da experiência.")]
    public TestDriveCompletedEvent TestDriveCompleted(TestDriveCompletedEvent @event) => @event;

    // ===== EVENTOS DE VENDA =====

    /// <summary>
    /// Evento publicado quando uma venda é fechada.
    /// </summary>
    [Channel("sale.closed", Description = "Canal para notificação de fechamento de vendas")]
    [PublishOperation(
        OperationId = "SaleClosedPublish",
        Summary = "Venda fechada",
        Description = "Publicado quando uma venda é concluída com sucesso. Dispara processos de faturamento, entrega e comissões.")]
    public SaleClosedEvent SaleClosed(SaleClosedEvent @event) => @event;

    // ===== EVENTOS DE AVALIAÇÃO DE SEMINOVOS =====

    /// <summary>
    /// Evento publicado quando uma avaliação de veículo usado é solicitada.
    /// </summary>
    [Channel("evaluation.requested", Description = "Canal para solicitação de avaliação de veículos usados")]
    [PublishOperation(
        OperationId = "UsedVehicleEvaluationRequestedPublish",
        Summary = "Avaliação de seminovo solicitada",
        Description = "Publicado quando um cliente solicita avaliação de seu veículo usado para troca. Integra com módulo de avaliação técnica.")]
    public UsedVehicleEvaluationRequestedEvent UsedVehicleEvaluationRequested(UsedVehicleEvaluationRequestedEvent @event) => @event;
}
