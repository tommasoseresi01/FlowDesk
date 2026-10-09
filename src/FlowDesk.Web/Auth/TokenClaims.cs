using System.Security.Claims;
using FlowDesk.Application.Models.Common;
using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Web.Auth;

// Estrae dal token di Entra ID ciò che serve all'applicazione. I token v1 e v2 usano nomi di claim
// diversi per le stesse informazioni, quindi per ognuna si prova più di un nome.
public static class TokenClaims
{
    private static readonly string[] ObjectIdClaims =
        ["oid", "http://schemas.microsoft.com/identity/claims/objectidentifier"];

    private static readonly string[] EmailClaims =
        ["preferred_username", "email", ClaimTypes.Email, "upn", ClaimTypes.Upn, "unique_name", ClaimTypes.Name];

    private static readonly string[] GivenNameClaims = ["given_name", ClaimTypes.GivenName];
    private static readonly string[] SurnameClaims = ["family_name", ClaimTypes.Surname];
    private static readonly string[] DisplayNameClaims = ["name"];

    // Gli App Roles assegnati all'utente arrivano nel claim "roles".
    private static readonly string[] RoleClaims = ["roles", ClaimTypes.Role];

    // Ritorna null se nel token mancano l'identificativo, l'email o un ruolo dell'applicazione.
    public static SignedInUser? Read(ClaimsPrincipal principal)
    {
        var objectId = FirstValue(principal, ObjectIdClaims);
        var email = FirstValue(principal, EmailClaims);
        var role = ReadRole(principal);

        if (!Guid.TryParse(objectId, out var entraObjectId) || string.IsNullOrWhiteSpace(email) || role is null)
        {
            return null;
        }

        var (name, surname) = ReadNames(principal, email);
        return new SignedInUser(entraObjectId, email.Trim(), name, surname, role.Value);
    }

    // I valori grezzi dei ruoli presenti nel token, anche se non sono ruoli di questa applicazione.
    // Serve alla diagnosi: gli App Roles non sono dati personali.
    public static IReadOnlyList<string> RoleValues(ClaimsPrincipal principal) =>
        RoleClaims.SelectMany(type => principal.FindAll(type)).Select(claim => claim.Value).Distinct().ToList();

    // Se l'utente ha più ruoli vale il più alto: ADMIN prima di MANAGER prima di OPERATOR.
    private static RoleEnum? ReadRole(ClaimsPrincipal principal)
    {
        var roles = RoleClaims
            .SelectMany(type => principal.FindAll(type))
            .Select(claim => Enum.TryParse<RoleEnum>(claim.Value, ignoreCase: true, out var role) ? role : (RoleEnum?)null)
            .Where(role => role is not null)
            .Select(role => role!.Value)
            .ToList();

        return roles.Count == 0 ? null : roles.Min();
    }

    private static (string Name, string Surname) ReadNames(ClaimsPrincipal principal, string email)
    {
        var givenName = FirstValue(principal, GivenNameClaims);
        var surname = FirstValue(principal, SurnameClaims);
        if (!string.IsNullOrWhiteSpace(givenName) || !string.IsNullOrWhiteSpace(surname))
        {
            return (givenName?.Trim() ?? string.Empty, surname?.Trim() ?? string.Empty);
        }

        // Senza nome e cognome separati si divide il nome visualizzato; in mancanza, si usa l'email.
        var displayName = FirstValue(principal, DisplayNameClaims);
        var source = string.IsNullOrWhiteSpace(displayName) ? email.Split('@')[0] : displayName.Trim();
        var parts = source.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length == 2 ? (parts[0], parts[1]) : (parts.FirstOrDefault() ?? string.Empty, string.Empty);
    }

    private static string? FirstValue(ClaimsPrincipal principal, IEnumerable<string> claimTypes) =>
        claimTypes
            .Select(type => principal.FindFirst(type)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
