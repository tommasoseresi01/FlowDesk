using System.Net.Http.Json;
using System.Text.Json;

namespace FlowDesk.IntegrationTests.Infrastructure;

public static class HttpExtensions
{
    // Legge il corpo come envelope JSON { success, result, errors, totResultNumber }.
    public static async Task<JsonElement> ReadEnvelopeAsync(this HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    public static IEnumerable<string> ErrorFields(this JsonElement envelope) =>
        envelope.GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("field").GetString() ?? string.Empty);

    public static Task<HttpResponseMessage> SearchCustomersAsync(
        this HttpClient client,
        string? legalName = null,
        string? status = null,
        string? sortColumn = null,
        bool descending = false,
        int start = 0,
        int size = 50)
    {
        var filters = new List<object>();
        if (legalName is not null) filters.Add(new { id = "legalNameFilter", value = legalName });
        if (status is not null) filters.Add(new { id = "statusFilter", value = status });

        var sorting = sortColumn is null
            ? Array.Empty<object>()
            : [new { id = sortColumn, desc = descending }];

        return client.PostAsJsonAsync("/api/customers/search", new
        {
            start,
            size,
            filters,
            globalFilter = string.Empty,
            sorting
        });
    }

    // Crea un cliente con una partita IVA casuale e restituisce l'envelope della risposta.
    public static async Task<(HttpResponseMessage Response, JsonElement Envelope)> CreateCustomerAsync(
        this HttpClient client,
        string legalName,
        string? vatNumber = null)
    {
        var response = await client.PostAsJsonAsync("/api/customers", new
        {
            legalName,
            vatNumber = vatNumber ?? RandomVatNumber(),
            contactName = "Irene Valli",
            email = "amministrazione@example.com",
            phone = "035 555 0142"
        });

        return (response, await response.ReadEnvelopeAsync());
    }

    public static string RandomVatNumber() =>
        Random.Shared.NextInt64(10_000_000_000, 100_000_000_000).ToString();

    public static string UniqueName(string prefix) => $"{prefix} {Guid.NewGuid():N}";
}
