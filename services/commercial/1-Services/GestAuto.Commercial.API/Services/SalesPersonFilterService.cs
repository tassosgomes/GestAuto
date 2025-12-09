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
        return Guid.TryParse(salesPersonIdClaim, out var id) ? id : null;
    }

    public bool IsManager()
    {
        var user = _contextAccessor.HttpContext?.User;
        return user?.HasClaim("role", "manager") ?? false;
    }

    public Guid GetCurrentUserId()
    {
        var user = _contextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}
