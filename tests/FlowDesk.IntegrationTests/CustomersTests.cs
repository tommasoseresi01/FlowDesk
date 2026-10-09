using System.Net;
using System.Net.Http.Json;
using FlowDesk.Domain.Entities.Enums;
using FlowDesk.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class CustomersTests(ApiFixture fixture)
{
    private HttpClient Manager => fixture.CreateClient(RoleEnum.MANAGER);
    private HttpClient Operator => fixture.CreateClient(RoleEnum.OPERATOR);

    private static int IdOf(System.Text.Json.JsonElement envelope) =>
        envelope.GetProperty("result").GetProperty("idCustomer").GetInt32();

    [Fact]
    public async Task Creating_a_customer_returns_it_with_the_audit_of_the_creator()
    {
        var name = HttpExtensions.UniqueName("Officine Meccaniche");

        var (response, envelope) = await Manager.CreateCustomerAsync(name, "01234567890".Replace('0', '7'));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(envelope.GetProperty("success").GetBoolean());

        var customer = envelope.GetProperty("result");
        Assert.True(customer.GetProperty("idCustomer").GetInt32() > 0);
        Assert.Equal(name, customer.GetProperty("legalName").GetString());
        Assert.False(customer.GetProperty("archived").GetBoolean());
        Assert.Equal(fixture.UserId(RoleEnum.MANAGER), customer.GetProperty("userModification").GetProperty("idUser").GetInt32());
        Assert.Equal(fixture.UserId(RoleEnum.MANAGER), customer.GetProperty("userCreation").GetProperty("idUser").GetInt32());

        // Le date viaggiano nel formato che il frontend legge: senza offset né frazioni di secondo.
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$", customer.GetProperty("dateModification").GetString());
    }

    [Fact]
    public async Task Editing_updates_the_data_and_the_audit_but_keeps_the_creator()
    {
        var (_, created) = await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Panificio"));
        var id = IdOf(created);
        var newName = HttpExtensions.UniqueName("Panificio rinominato");

        var response = await Operator.PutAsJsonAsync("/api/customers", new
        {
            idCustomer = id,
            legalName = newName,
            vatNumber = created.GetProperty("result").GetProperty("vatNumber").GetString(),
            contactName = "Nuovo referente",
            email = string.Empty,
            phone = string.Empty
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = (await response.ReadEnvelopeAsync()).GetProperty("result");
        Assert.Equal(newName, customer.GetProperty("legalName").GetString());
        Assert.Equal("Nuovo referente", customer.GetProperty("contactName").GetString());
        Assert.Equal(fixture.UserId(RoleEnum.OPERATOR), customer.GetProperty("userModification").GetProperty("idUser").GetInt32());
        Assert.Equal(fixture.UserId(RoleEnum.MANAGER), customer.GetProperty("userCreation").GetProperty("idUser").GetInt32());
    }

    [Fact]
    public async Task A_duplicate_vat_number_is_rejected_on_the_vat_field()
    {
        var vat = HttpExtensions.RandomVatNumber();
        await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Primo"), vat);

        var (response, envelope) = await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Secondo"), vat);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.False(envelope.GetProperty("success").GetBoolean());
        Assert.Contains("VatNumber", envelope.ErrorFields());
    }

    [Fact]
    public async Task Editing_into_the_vat_number_of_another_customer_is_rejected()
    {
        var vat = HttpExtensions.RandomVatNumber();
        await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Esistente"), vat);
        var (_, other) = await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Altro"));

        var response = await Manager.PutAsJsonAsync("/api/customers", new
        {
            idCustomer = IdOf(other),
            legalName = HttpExtensions.UniqueName("Altro"),
            vatNumber = vat
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("VatNumber", (await response.ReadEnvelopeAsync()).ErrorFields());
    }

    [Fact]
    public async Task Invalid_data_returns_400_with_the_errors_per_field()
    {
        var response = await Manager.PostAsJsonAsync("/api/customers", new
        {
            legalName = string.Empty,
            vatNumber = "123",
            email = "not-an-email"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var envelope = await response.ReadEnvelopeAsync();
        Assert.False(envelope.GetProperty("success").GetBoolean());

        // Il frontend porta in minuscolo la prima lettera e riporta l'errore sul campo omonimo del form.
        var fields = envelope.ErrorFields().ToList();
        Assert.Contains("LegalName", fields);
        Assert.Contains("VatNumber", fields);
        Assert.Contains("Email", fields);
    }

    [Fact]
    public async Task A_malformed_body_returns_400_with_the_standard_envelope()
    {
        var response = await Manager.PostAsync(
            "/api/customers",
            new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False((await response.ReadEnvelopeAsync()).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task A_missing_customer_returns_404_with_the_standard_envelope()
    {
        var response = await Manager.GetAsync("/api/customers/2147000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False((await response.ReadEnvelopeAsync()).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task An_archived_customer_leaves_the_active_list_and_enters_the_archived_one()
    {
        var tag = HttpExtensions.UniqueName("Archiviazione");
        var (_, created) = await Manager.CreateCustomerAsync(tag);
        var id = IdOf(created);

        await Manager.DeleteAsync($"/api/customers/{id}");

        Assert.Equal(0, await CountAsync(tag, "active"));
        Assert.Equal(1, await CountAsync(tag, "archived"));
        Assert.Equal(1, await CountAsync(tag, string.Empty));

        var customer = (await (await Manager.GetAsync($"/api/customers/{id}")).ReadEnvelopeAsync()).GetProperty("result");
        Assert.True(customer.GetProperty("archived").GetBoolean());
    }

    [Fact]
    public async Task Archiving_records_who_and_when_and_restoring_clears_it()
    {
        var (_, created) = await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Audit"));
        var id = IdOf(created);

        await Manager.DeleteAsync($"/api/customers/{id}");
        var archived = await fixture.QueryDatabaseAsync(db => db.Customers.AsNoTracking().SingleAsync(c => c.IdCustomer == id));
        Assert.True(archived.Deleted);
        Assert.Equal(fixture.UserId(RoleEnum.MANAGER), archived.IdUserDeleted);
        Assert.NotNull(archived.DateDeleted);

        await Manager.PostAsync($"/api/customers/{id}/restore", null);
        var restored = await fixture.QueryDatabaseAsync(db => db.Customers.AsNoTracking().SingleAsync(c => c.IdCustomer == id));
        Assert.False(restored.Deleted);
        Assert.Null(restored.IdUserDeleted);
        Assert.Null(restored.DateDeleted);
    }

    [Fact]
    public async Task Archiving_twice_and_restoring_an_active_customer_are_rejected()
    {
        var (_, created) = await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Doppio"));
        var id = IdOf(created);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await Manager.PostAsync($"/api/customers/{id}/restore", null)).StatusCode);

        await Manager.DeleteAsync($"/api/customers/{id}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await Manager.DeleteAsync($"/api/customers/{id}")).StatusCode);
    }

    [Fact]
    public async Task An_archived_customer_cannot_be_edited_until_it_is_restored()
    {
        var (_, created) = await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Bloccato"));
        var id = IdOf(created);
        var vat = created.GetProperty("result").GetProperty("vatNumber").GetString();
        await Manager.DeleteAsync($"/api/customers/{id}");

        var rejected = await Manager.PutAsJsonAsync("/api/customers", new { idCustomer = id, legalName = "Nuovo nome", vatNumber = vat });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);

        await Manager.PostAsync($"/api/customers/{id}/restore", null);
        var accepted = await Manager.PutAsJsonAsync("/api/customers", new { idCustomer = id, legalName = "Nuovo nome", vatNumber = vat });
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
    }

    [Fact]
    public async Task Search_paginates_and_reports_the_total()
    {
        var tag = HttpExtensions.UniqueName("Paginazione");
        foreach (var number in Enumerable.Range(1, 5))
        {
            await Manager.CreateCustomerAsync($"{tag} {number}");
        }

        var response = await Manager.SearchCustomersAsync(legalName: tag, sortColumn: "legalName", start: 2, size: 2);

        var envelope = await response.ReadEnvelopeAsync();
        Assert.Equal(5, envelope.GetProperty("totResultNumber").GetInt32());
        var names = envelope.GetProperty("result").EnumerateArray().Select(c => c.GetProperty("legalName").GetString()).ToList();
        Assert.Equal([$"{tag} 3", $"{tag} 4"], names);
    }

    [Fact]
    public async Task Search_sorts_descending()
    {
        var tag = HttpExtensions.UniqueName("Ordine");
        foreach (var number in Enumerable.Range(1, 3))
        {
            await Manager.CreateCustomerAsync($"{tag} {number}");
        }

        var response = await Manager.SearchCustomersAsync(legalName: tag, sortColumn: "legalName", descending: true);

        var names = (await response.ReadEnvelopeAsync()).GetProperty("result").EnumerateArray()
            .Select(c => c.GetProperty("legalName").GetString()).ToList();
        Assert.Equal([$"{tag} 3", $"{tag} 2", $"{tag} 1"], names);
    }

    [Fact]
    public async Task Search_can_sort_by_the_archived_column()
    {
        var tag = HttpExtensions.UniqueName("Stato");
        await Manager.CreateCustomerAsync($"{tag} attivo");
        var (_, archived) = await Manager.CreateCustomerAsync($"{tag} archiviato");
        await Manager.DeleteAsync($"/api/customers/{IdOf(archived)}");

        var response = await Manager.SearchCustomersAsync(legalName: tag, status: string.Empty, sortColumn: "archived", descending: true);

        var first = (await response.ReadEnvelopeAsync()).GetProperty("result")[0];
        Assert.True(first.GetProperty("archived").GetBoolean());
    }

    [Fact]
    public async Task An_unknown_sort_column_falls_back_to_the_default_order_instead_of_failing()
    {
        var response = await Manager.SearchCustomersAsync(sortColumn: "doesNotExist");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Search_filters_by_vat_number()
    {
        var vat = HttpExtensions.RandomVatNumber();
        await Manager.CreateCustomerAsync(HttpExtensions.UniqueName("Per IVA"), vat);

        var response = await Manager.PostAsJsonAsync("/api/customers/search", new
        {
            start = 0,
            size = 10,
            filters = new[] { new { id = "vatNumberFilter", value = vat } },
            globalFilter = string.Empty,
            sorting = Array.Empty<object>()
        });

        Assert.Equal(1, (await response.ReadEnvelopeAsync()).GetProperty("totResultNumber").GetInt32());
    }

    private async Task<int> CountAsync(string legalName, string status)
    {
        var response = await Manager.SearchCustomersAsync(legalName, status);
        return (await response.ReadEnvelopeAsync()).GetProperty("totResultNumber").GetInt32();
    }
}
