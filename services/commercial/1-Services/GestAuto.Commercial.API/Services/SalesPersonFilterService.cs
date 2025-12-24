using System.Security.Claims;

namespace GestAuto.Commercial.API.Services;

public interface ISalesPersonFilterService
{
    Guid? GetCurrentSalesPersonId();
    bool IsManager();
    Guid GetCurrentUserId();
}

public class SalesPersonFilterService : ISalesPersonFilterService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public SalesPersonFilterService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Guid? GetCurrentSalesPersonId()
    {
        var user = _contextAccessor.HttpContext?.User;
        if (user == null) return null;

        // Gerente não tem filtro por vendedor
        if (IsManager()) return null;

        var salesPersonIdClaim = user.FindFirst("sales_person_id")?.Value;
        if (Guid.TryParse(salesPersonIdClaim, out var id))
        {
            return id;
        }

        // Fallback: se não houver claim específica, assume que o ID do vendedor é o ID do usuário
        return GetCurrentUserId();
    }

    public bool IsManager()
    {
        var user = _contextAccessor.HttpContext?.User;
        // Roles padronizadas conforme ROLES_NAMING_CONVENTION.md
        return user?.HasClaim("roles", "MANAGER") == true ||
               user?.HasClaim("roles", "SALES_MANAGER") == true ||
               user?.HasClaim("roles", "ADMIN") == true;
    }

    public Guid GetCurrentUserId()
    {
        var user = _contextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}
