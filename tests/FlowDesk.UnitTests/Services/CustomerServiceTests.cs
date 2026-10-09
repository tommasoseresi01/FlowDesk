using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Exceptions;
using FlowDesk.Application.Models.Common;
using FlowDesk.Application.Models.Requests;
using FlowDesk.Application.Models.Requests.Customers;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using NSubstitute;

namespace FlowDesk.UnitTests.Services;

public class CustomerServiceTests
{
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.CustomerRepository.Returns(_repository);
        _service = new CustomerService(unitOfWork);
    }

    [Fact]
    public async Task Create_trims_the_fields_before_saving()
    {
        _repository.CreateAsync(Arg.Any<Customer>()).Returns(call => call.Arg<Customer>());

        await _service.CreateCustomerAsync(new CreateCustomerRequest
        {
            LegalName = "  Panificio Delle Valli S.n.c.  ",
            VatNumber = " 01234567890 ",
            Email = " amministrazione@example.com "
        });

        await _repository.Received(1).CreateAsync(Arg.Is<Customer>(c =>
            c.LegalName == "Panificio Delle Valli S.n.c."
            && c.VatNumber == "01234567890"
            && c.Email == "amministrazione@example.com"));
    }

    [Fact]
    public async Task Create_rejects_a_duplicate_vat_number_on_the_vat_field()
    {
        _repository.VatNumberExistsAsync("01234567890", null).Returns(true);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            _service.CreateCustomerAsync(new CreateCustomerRequest
            {
                LegalName = "Ottica Vescovi",
                VatNumber = "01234567890"
            }));

        Assert.Equal("VatNumber", Assert.Single(exception.Errors).Field);
        await _repository.DidNotReceive().CreateAsync(Arg.Any<Customer>());
    }

    [Fact]
    public async Task Edit_fails_when_the_customer_does_not_exist()
    {
        _repository.GetByIdAsync(99).Returns((Customer?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.EditCustomerAsync(new EditCustomerRequest { IdCustomer = 99, LegalName = "X", VatNumber = "01234567890" }));
    }

    [Fact]
    public async Task Edit_refuses_an_archived_customer()
    {
        _repository.GetByIdAsync(5).Returns(new Customer { IdCustomer = 5, Deleted = true });

        await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            _service.EditCustomerAsync(new EditCustomerRequest { IdCustomer = 5, LegalName = "X", VatNumber = "01234567890" }));

        await _repository.DidNotReceive().EditAsync(Arg.Any<Customer>());
    }

    [Fact]
    public async Task Edit_checks_the_vat_number_excluding_the_customer_itself()
    {
        _repository.GetByIdAsync(5).Returns(new Customer { IdCustomer = 5 });
        _repository.VatNumberExistsAsync("01234567890", 5).Returns(false);
        _repository.EditAsync(Arg.Any<Customer>()).Returns(call => call.Arg<Customer>());

        await _service.EditCustomerAsync(new EditCustomerRequest { IdCustomer = 5, LegalName = "X", VatNumber = "01234567890" });

        await _repository.Received(1).VatNumberExistsAsync("01234567890", 5);
        await _repository.Received(1).EditAsync(Arg.Any<Customer>());
    }

    [Fact]
    public async Task Archive_refuses_a_customer_that_is_already_archived()
    {
        _repository.GetByIdAsync(5).Returns(new Customer { IdCustomer = 5, Deleted = true });

        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.ArchiveCustomerAsync(5));

        await _repository.DidNotReceive().ArchiveAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Archive_archives_an_active_customer()
    {
        _repository.GetByIdAsync(5).Returns(new Customer { IdCustomer = 5 });

        await _service.ArchiveCustomerAsync(5);

        await _repository.Received(1).ArchiveAsync(5);
    }

    [Fact]
    public async Task Restore_refuses_a_customer_that_is_not_archived()
    {
        _repository.GetByIdAsync(5).Returns(new Customer { IdCustomer = 5, Deleted = false });

        await Assert.ThrowsAsync<ApplicationValidationException>(() => _service.RestoreCustomerAsync(5));

        await _repository.DidNotReceive().RestoreAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Search_translates_the_archived_column_to_the_deleted_property()
    {
        _repository
            .GetCustomersAsync(default!, default!, default!, default, default, default)
            .ReturnsForAnyArgs(new PaginatedList<Customer>());

        await _service.SearchCustomersAsync(new CustomerSearchRequest
        {
            Sorting = [new MaterialReactTableSorting { Id = "archived", Desc = true }]
        });

        await _repository.Received(1).GetCustomersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<SortingInfo?>(s => s != null
                && s.ColumnName == "Deleted"
                && s.SortDirection == SortDirectionEnum.DESCENDING),
            Arg.Any<int>(),
            Arg.Any<int>());
    }
}
