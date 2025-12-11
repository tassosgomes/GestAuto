using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GestAuto.Commercial.Tests.Shared;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Roles padronizadas conforme ROLES_NAMING_CONVENTION.md
        var role = Request.Headers.TryGetValue("X-Test-Role", out var roles)
            ? roles.ToString()
            : "SALES_PERSON";

        var salesPersonId = Request.Headers.TryGetValue("X-Test-SalesPersonId", out var salesPerson)
            ? salesPerson.ToString()
            : Guid.NewGuid().ToString();

        var subject = Request.Headers.TryGetValue("X-Test-UserId", out var userIds)
            ? userIds.ToString()
            : Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.NameIdentifier, subject),
            new("sub", subject),
            new("sales_person_id", salesPersonId),
            new("roles", role) // Claim padronizada
        };

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
