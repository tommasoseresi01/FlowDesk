using System.Net;
using System.Net.Http.Json;
using FlowDesk.Domain.Entities.Enums;
using FlowDesk.IntegrationTests.Infrastructure;

namespace FlowDesk.IntegrationTests;

// La matrice dei permessi: chi può fare cosa sui clienti.
[Collection(ApiCollection.Name)]
public class CustomersAuthorizationTests(ApiFixture fixture)
{
    [Fact]
    public async Task The_admin_cannot_work_on_customers()
    {
        var client = fixture.CreateClient(RoleEnum.ADMIN);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.SearchCustomersAsync()).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/customers/1")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.PostAsJsonAsync("/api/customers", new { legalName = "X", vatNumber = "01234567890" })).StatusCode);
    }

    [Theory]
    [InlineData(RoleEnum.MANAGER)]
    [InlineData(RoleEnum.OPERATOR)]
    public async Task Managers_and_operators_can_search_create_and_edit(RoleEnum role)
    {
        var client = fixture.CreateClient(role);

        Assert.Equal(HttpStatusCode.OK, (await client.SearchCustomersAsync()).StatusCode);

        var (created, envelope) = await client.CreateCustomerAsync(HttpExtensions.UniqueName("Permessi"));
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);

        var id = envelope.GetProperty("result").GetProperty("idCustomer").GetInt32();
        var edit = await client.PutAsJsonAsync("/api/customers", new
        {
            idCustomer = id,
            legalName = HttpExtensions.UniqueName("Permessi modificato"),
            vatNumber = HttpExtensions.RandomVatNumber()
        });
        Assert.Equal(HttpStatusCode.OK, edit.StatusCode);
    }

    [Fact]
    public async Task An_operator_cannot_archive_or_restore()
    {
        var manager = fixture.CreateClient(RoleEnum.MANAGER);
        var operatorClient = fixture.CreateClient(RoleEnum.OPERATOR);
        var (_, envelope) = await manager.CreateCustomerAsync(HttpExtensions.UniqueName("Archivio"));
        var id = envelope.GetProperty("result").GetProperty("idCustomer").GetInt32();

        Assert.Equal(HttpStatusCode.Forbidden, (await operatorClient.DeleteAsync($"/api/customers/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await operatorClient.PostAsync($"/api/customers/{id}/restore", null)).StatusCode);

        // Il cliente è rimasto attivo.
        var customer = await manager.GetAsync($"/api/customers/{id}");
        var archived = (await customer.ReadEnvelopeAsync()).GetProperty("result").GetProperty("archived").GetBoolean();
        Assert.False(archived);
    }

    [Fact]
    public async Task A_manager_can_archive_and_restore()
    {
        var manager = fixture.CreateClient(RoleEnum.MANAGER);
        var (_, envelope) = await manager.CreateCustomerAsync(HttpExtensions.UniqueName("Archivio"));
        var id = envelope.GetProperty("result").GetProperty("idCustomer").GetInt32();

        Assert.Equal(HttpStatusCode.OK, (await manager.DeleteAsync($"/api/customers/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.PostAsync($"/api/customers/{id}/restore", null)).StatusCode);
    }
}
