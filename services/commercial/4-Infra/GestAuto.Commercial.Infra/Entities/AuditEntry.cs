using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Infra.Entities;

public class AuditEntry : BaseEntity
{
    public string EntityName { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = null!; // CREATE, UPDATE, DELETE
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    
    public AuditEntry(
        string entityName,
        Guid entityId, 
        string action,
        string? oldValues,
        string? newValues,
        Guid userId,
        string userName)
    {
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        OldValues = oldValues;
        NewValues = newValues;
        UserId = userId;
        UserName = userName;
    }

    private AuditEntry() { } // EF Core
}