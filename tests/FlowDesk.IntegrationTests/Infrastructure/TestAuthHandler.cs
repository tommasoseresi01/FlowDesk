using System.Security.Claims;
using System.Text.Encodings.Web;
using FlowDesk.Application.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlowDesk.IntegrationTests.Infrastructure;

// Sostituisce il login Microsoft nei test: l'identità arriva da due header.
// Emette gli stessi claim che la pipeline reale aggiunge dopo aver validato il token.
// Senza header la richiesta è anonima.
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RoleHeader = "X-Test-Role";
    public const string EmailHeader = "X-Test-Email";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userId)
            || !Request.Headers.TryGetValue(RoleHeader, out var role))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var roleId = (int)Enum.Parse<FlowDesk.Domain.Entities.Enums.RoleEnum>(role.ToString());
        var claims = new[]
        {
            new Claim(ClaimsNames.USER_ID, userId.ToString()),
            new Claim(ClaimsNames.USER_EMAIL, Request.Headers[EmailHeader].ToString()),
            new Claim(ClaimsNames.ROLE_ID, roleId.ToString()),
            new Claim(ClaimsNames.ROLE_NAME, role.ToString())
        };

        var identity = new ClaimsIdentity(claims, SchemeName, nameType: ClaimsNames.USER_EMAIL, roleType: ClaimsNames.ROLE_NAME);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
