using FlowDesk.Application.Models.Requests.Customers;
using FlowDesk.Application.Validators;

namespace FlowDesk.UnitTests.Validators;

public class CustomerRequestValidatorTests
{
    private readonly CreateCustomerRequestValidator _create = new();
    private readonly EditCustomerRequestValidator _edit = new();

    private static CreateCustomerRequest ValidCreate() => new()
    {
        LegalName = "Officine Meccaniche Brembati S.r.l.",
        VatNumber = "01234567890",
        ContactName = "Irene Valli",
        Email = "amministrazione@example.com",
        Phone = "035 555 0142"
    };

    [Fact]
    public void A_complete_request_is_valid()
    {
        Assert.True(_create.Validate(ValidCreate()).IsValid);
    }

    [Fact]
    public void Only_legal_name_and_vat_number_are_required()
    {
        var request = new CreateCustomerRequest { LegalName = "Ottica Vescovi", VatNumber = "09876543210" };

        Assert.True(_create.Validate(request).IsValid);
    }

    [Fact]
    public void An_empty_legal_name_is_rejected()
    {
        var request = ValidCreate();
        request.LegalName = " ";

        var result = _create.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "LegalName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("0123456789")]
    [InlineData("012345678901")]
    [InlineData("0123456789A")]
    public void A_vat_number_that_is_not_eleven_digits_is_rejected(string vatNumber)
    {
        var request = ValidCreate();
        request.VatNumber = vatNumber;

        var result = _create.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "VatNumber");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    public void An_invalid_email_is_rejected(string email)
    {
        var request = ValidCreate();
        request.Email = email;

        var result = _create.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void An_empty_email_is_accepted()
    {
        var request = ValidCreate();
        request.Email = string.Empty;

        Assert.True(_create.Validate(request).IsValid);
    }

    [Fact]
    public void Fields_longer_than_the_database_columns_are_rejected()
    {
        var request = ValidCreate();
        request.LegalName = new string('a', 201);
        request.ContactName = new string('b', 121);
        request.Phone = new string('1', 31);

        var fields = _create.Validate(request).Errors.Select(e => e.PropertyName).ToList();

        Assert.Contains("LegalName", fields);
        Assert.Contains("ContactName", fields);
        Assert.Contains("Phone", fields);
    }

    [Fact]
    public void Editing_requires_a_valid_customer_id()
    {
        var request = new EditCustomerRequest
        {
            IdCustomer = 0,
            LegalName = "Ottica Vescovi",
            VatNumber = "09876543210"
        };

        var result = _edit.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "IdCustomer");
    }

    [Fact]
    public void Editing_applies_the_same_rules_as_creating()
    {
        var request = new EditCustomerRequest { IdCustomer = 3, LegalName = "Ottica Vescovi", VatNumber = "abc" };

        var result = _edit.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "VatNumber");
    }
}
