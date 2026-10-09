using System.Security.Claims;
using FlowDesk.Domain.Entities.Enums;
using FlowDesk.Web.Auth;

namespace FlowDesk.IntegrationTests;

// I token di Entra ID v1 e v2 usano nomi di claim diversi: qui si verifica che si leggano entrambi.
public class TokenClaimsTests
{
    private static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static ClaimsPrincipal Principal(params (string Type, string Value)[] claims) =>
        new(new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), "test"));

    [Fact]
    public void A_v2_token_is_read()
    {
        var user = TokenClaims.Read(Principal(
            ("oid", ObjectId.ToString()),
            ("preferred_username", "marta.belloni@example.com"),
            ("given_name", "Marta"),
            ("family_name", "Belloni"),
            ("roles", "Manager")));

        Assert.NotNull(user);
        Assert.Equal(ObjectId, user.EntraObjectId);
        Assert.Equal("marta.belloni@example.com", user.Email);
        Assert.Equal("Marta", user.Name);
        Assert.Equal("Belloni", user.Surname);
        Assert.Equal(RoleEnum.MANAGER, user.Role);
    }

    [Fact]
    public void A_v1_token_with_mapped_claim_types_is_read()
    {
        var user = TokenClaims.Read(Principal(
            ("http://schemas.microsoft.com/identity/claims/objectidentifier", ObjectId.ToString()),
            (ClaimTypes.Name, "davide.ferraro@example.com"),
            (ClaimTypes.GivenName, "Davide"),
            (ClaimTypes.Surname, "Ferraro"),
            (ClaimTypes.Role, "Operator")));

        Assert.NotNull(user);
        Assert.Equal("davide.ferraro@example.com", user.Email);
        Assert.Equal(RoleEnum.OPERATOR, user.Role);
    }

    [Fact]
    public void Role_names_are_matched_ignoring_the_case()
    {
        var user = TokenClaims.Read(Principal(
            ("oid", ObjectId.ToString()),
            ("preferred_username", "elena@example.com"),
            ("roles", "admin")));

        Assert.Equal(RoleEnum.ADMIN, user!.Role);
    }

    [Fact]
    public void With_more_roles_the_highest_one_wins()
    {
        var user = TokenClaims.Read(Principal(
            ("oid", ObjectId.ToString()),
            ("preferred_username", "elena@example.com"),
            ("roles", "Operator"),
            ("roles", "Manager"),
            ("roles", "Unrelated")));

        Assert.Equal(RoleEnum.MANAGER, user!.Role);
    }

    [Fact]
    public void Without_an_application_role_there_is_no_user()
    {
        var user = TokenClaims.Read(Principal(
            ("oid", ObjectId.ToString()),
            ("preferred_username", "elena@example.com"),
            ("roles", "SomethingElse")));

        Assert.Null(user);
    }

    [Fact]
    public void Without_a_valid_object_id_there_is_no_user()
    {
        var user = TokenClaims.Read(Principal(
            ("oid", "not-a-guid"),
            ("preferred_username", "elena@example.com"),
            ("roles", "Admin")));

        Assert.Null(user);
    }

    [Fact]
    public void Without_an_email_there_is_no_user()
    {
        var user = TokenClaims.Read(Principal(("oid", ObjectId.ToString()), ("roles", "Admin")));

        Assert.Null(user);
    }

    [Fact]
    public void The_display_name_is_split_when_given_and_family_names_are_missing()
    {
        var user = TokenClaims.Read(Principal(
            ("oid", ObjectId.ToString()),
            ("preferred_username", "sara.colombo@example.com"),
            ("name", "Sara Colombo De Luca"),
            ("roles", "Operator")));

        Assert.Equal("Sara", user!.Name);
        Assert.Equal("Colombo De Luca", user.Surname);
    }

    [Fact]
    public void Without_any_name_the_email_prefix_is_used()
    {
        var user = TokenClaims.Read(Principal(
            ("oid", ObjectId.ToString()),
            ("preferred_username", "luca@example.com"),
            ("roles", "Operator")));

        Assert.Equal("luca", user!.Name);
        Assert.Equal(string.Empty, user.Surname);
    }
}
