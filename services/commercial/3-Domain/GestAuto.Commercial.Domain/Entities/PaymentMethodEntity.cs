namespace GestAuto.Commercial.Domain.Entities;

/// <summary>
/// Entidade que representa uma forma de pagamento disponível no sistema.
/// Permite gestão dinâmica das opções de pagamento sem necessidade de recompilação.
/// </summary>
public class PaymentMethodEntity
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private PaymentMethodEntity() { }

    public PaymentMethodEntity(
        int id,
        string code,
        string name,
        bool isActive,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code não pode ser vazio", nameof(code));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name não pode ser vazio", nameof(name));

        Id = id;
        Code = code;
        Name = name;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, bool isActive, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name não pode ser vazio", nameof(name));

        Name = name;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
