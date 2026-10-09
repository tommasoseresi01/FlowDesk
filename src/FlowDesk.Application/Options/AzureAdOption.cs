using System.ComponentModel.DataAnnotations;

namespace FlowDesk.Application.Options;

// Sezione "AzureAd". TenantId e ClientId non stanno in appsettings.json: arrivano dai
// user secrets in locale e dalle impostazioni dell'App Service negli ambienti.
public class AzureAdOption
{
    public const string SectionName = "AzureAd";

    [Required]
    public string Instance { get; set; } = "https://login.microsoftonline.com";

    [Required]
    public string TenantId { get; set; } = string.Empty;

    // ID applicazione (client) della registrazione che espone l'API.
    [Required]
    public string ClientId { get; set; } = string.Empty;
}
