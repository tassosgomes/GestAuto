using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// DTO de resposta para forma de pagamento
/// </summary>
public record PaymentMethodResponse(
    int Id,
    string Code,
    string Name,
    bool IsActive,
    int DisplayOrder
)
{
    public static PaymentMethodResponse FromEntity(PaymentMethodEntity entity)
    {
        return new PaymentMethodResponse(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.IsActive,
            entity.DisplayOrder
        );
    }
}
