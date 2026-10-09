using System.Security.Claims;
using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Models.Responses;
using FlowDesk.Application.Options;
using FlowDesk.Application.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlowDesk.Web.Auth;

public static class JwtAuthenticationExtensions
{
    // Entra ID emette i token, il backend li valida soltanto: non esistono endpoint di login.
    // Il token dice chi è l'utente e quale App Role ha; dopo la validazione l'utente viene registrato
    // (o aggiornato) nella tabella Users e al principal si aggiungono i claim applicativi.
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Le impostazioni si leggono dalle options, così valgono quelle validate all'avvio.
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AzureAdOption>>((options, azureAdOptions) =>
            {
                var azureAd = azureAdOptions.Value;
                var instance = azureAd.Instance.TrimEnd('/');

                options.Authority = $"{instance}/{azureAd.TenantId}/v2.0";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Un'app registration può emettere token v1 o v2: l'issuer e l'audience cambiano forma.
                    ValidIssuers =
                    [
                        $"https://sts.windows.net/{azureAd.TenantId}/",
                        $"{instance}/{azureAd.TenantId}/v2.0"
                    ],
                    ValidAudiences = [$"api://{azureAd.ClientId}", azureAd.ClientId],
                    RoleClaimType = ClaimsNames.ROLE_NAME
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = OnTokenValidatedAsync,
                    OnChallenge = OnChallengeAsync,
                    OnForbidden = OnForbiddenAsync
                };
            });

        return services;
    }

    private static async Task OnTokenValidatedAsync(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(JwtAuthenticationExtensions));

        var signedInUser = TokenClaims.Read(context.Principal);
        if (signedInUser is null)
        {
            // Il token è firmato bene ma non basta: si registra cosa c'era (solo i tipi di claim e i
            // ruoli, mai i valori personali) per capire se manca l'App Role, l'oid o l'email.
            logger.LogWarning(
                "Token valido ma senza i dati applicativi. Tipi di claim presenti: {ClaimTypes}. Ruoli nel token: {Roles}",
                string.Join(", ", context.Principal.Claims.Select(claim => claim.Type).Distinct()),
                string.Join(", ", TokenClaims.RoleValues(context.Principal)));

            context.Fail("Il token non contiene l'identificativo, l'email o un ruolo dell'applicazione.");
            return;
        }

        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var user = await userService.SignInAsync(signedInUser);
        if (!user.Enabled)
        {
            logger.LogWarning("Accesso negato: l'utente {IdUser} è disattivato", user.IdUser);
            context.Fail("Utente disattivato.");
            return;
        }

        identity.AddClaim(new Claim(ClaimsNames.USER_ID, user.IdUser.ToString()));
        identity.AddClaim(new Claim(ClaimsNames.USER_EMAIL, user.Email));
        identity.AddClaim(new Claim(ClaimsNames.ROLE_ID, ((int)user.IdRole).ToString()));
        identity.AddClaim(new Claim(ClaimsNames.ROLE_NAME, user.IdRole.ToString()));
    }

    // 401 e 403 rispondono con l'envelope JSON: il frontend legge sempre il corpo come JSON.
    private static Task OnChallengeAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        return WriteErrorAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Autenticazione richiesta o non valida.");
    }

    private static Task OnForbiddenAsync(ForbiddenContext context) =>
        WriteErrorAsync(context.HttpContext, StatusCodes.Status403Forbidden, "Non hai i permessi per eseguire questa operazione.");

    private static Task WriteErrorAsync(HttpContext httpContext, int status, string message)
    {
        var response = new ErrorResponse();
        response.WithError(string.Empty, message);

        httpContext.Response.StatusCode = status;
        return httpContext.Response.WriteAsJsonAsync(response);
    }
}
