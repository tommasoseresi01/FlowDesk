using System.Net;
using FlowDesk.Domain.Entities.Enums;
using FlowDesk.IntegrationTests.Infrastructure;

namespace FlowDesk.IntegrationTests;

[Collection(ApiCollection.Name)]
public class UsersTests(ApiFixture fixture)
{
    [Theory]
    [InlineData("/api/users/current")]
    [InlineData("/api/users/menu")]
    [InlineData("/api/customers/1")]
    public async Task Anonymous_requests_are_rejected(string url)
    {
        var response = await fixture.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/users/current")]
    [InlineData("/api/v1.0/users/current")]
    public async Task Current_user_answers_on_both_route_prefixes(string url)
    {
        var response = await fixture.CreateClient(RoleEnum.MANAGER).GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(RoleEnum.ADMIN, 1)]
    [InlineData(RoleEnum.MANAGER, 2)]
    [InlineData(RoleEnum.OPERATOR, 3)]
    public async Task Current_user_has_the_shape_the_frontend_expects(RoleEnum role, int expectedRoleId)
    {
        var response = await fixture.CreateClient(role).GetAsync("/api/users/current");

        var envelope = await response.ReadEnvelopeAsync();
        Assert.True(envelope.GetProperty("success").GetBoolean());

        var user = envelope.GetProperty("result");
        Assert.Equal(fixture.UserId(role), user.GetProperty("idUser").GetInt32());
        Assert.Equal($"{role.ToString().ToLowerInvariant()}@test.example", user.GetProperty("email").GetString());
        Assert.True(user.GetProperty("enabled").GetBoolean());
        Assert.Equal(expectedRoleId, user.GetProperty("role").GetProperty("idRole").GetInt32());
        Assert.Equal(role.ToString(), user.GetProperty("role").GetProperty("roleName").GetString());
    }

    [Theory]
    [InlineData(RoleEnum.ADMIN, new[] { "/home" })]
    [InlineData(RoleEnum.MANAGER, new[] { "/home", "/customers" })]
    [InlineData(RoleEnum.OPERATOR, new[] { "/home", "/customers" })]
    public async Task Menu_depends_on_the_role(RoleEnum role, string[] expectedPaths)
    {
        var response = await fixture.CreateClient(role).GetAsync("/api/users/menu");

        var envelope = await response.ReadEnvelopeAsync();
        var paths = envelope.GetProperty("result").EnumerateArray()
            .Select(item => item.GetProperty("path").GetString())
            .ToArray();

        Assert.Equal(expectedPaths, paths);
    }
}
