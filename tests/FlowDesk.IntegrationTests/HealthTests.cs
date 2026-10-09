using System.Net;
using FlowDesk.IntegrationTests.Infrastructure;

namespace FlowDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class HealthTests(ApiFixture fixture)
{
    [Fact]
    public async Task Live_answers_without_authentication()
    {
        var response = await fixture.CreateClient().GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_checks_that_the_database_is_reachable()
    {
        var response = await fixture.CreateClient().GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
