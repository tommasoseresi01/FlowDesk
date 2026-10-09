using FlowDesk.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class EndpointSecurityTests(ApiFixture fixture)
{
    private const string HealthPrefix = "health/";

    // Rete di sicurezza: un controller aggiunto senza [Authorize] resta protetto dalla policy di default,
    // ma un endpoint reso anonimo per sbaglio deve far fallire questo test.
    [Fact]
    public void Only_the_health_checks_are_anonymous()
    {
        var endpoints = fixture.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>();

        var anonymous = endpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Select(endpoint => endpoint.RoutePattern.RawText?.TrimStart('/'))
            .ToList();

        Assert.NotEmpty(anonymous);
        Assert.All(anonymous, route => Assert.StartsWith(HealthPrefix, route));
    }

    [Fact]
    public void Every_controller_endpoint_is_protected()
    {
        var endpoints = fixture.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText?.TrimStart('/').StartsWith("api/", StringComparison.Ordinal) == true)
            .ToList();

        // Due prefissi (api/ e api/v1.0/) per ogni azione dei due controller.
        Assert.True(endpoints.Count >= 10, $"Trovati solo {endpoints.Count} endpoint API");
        Assert.All(endpoints, endpoint =>
            Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()));
    }
}
