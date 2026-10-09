using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Exceptions;
using FlowDesk.Application.Mappers;
using FlowDesk.Application.Models.Common;
using FlowDesk.Application.Models.Requests.Customers;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Services;

public class CustomerService(IUnitOfWork unitOfWork) : ICustomerService
{
    private const string VatNumberField = "VatNumber";
    private const string GeneralField = "";

    // Il frontend ordina per "archived", che nel modello è la cancellazione logica.
    private const string ArchivedColumn = "archived";
    private const string DeletedColumn = nameof(Customer.Deleted);

    public async Task<PaginatedList<Customer>> SearchCustomersAsync(CustomerSearchRequest request)
    {
        var sorting = request.ToSortingInfo();
        if (sorting is not null
            && string.Equals(sorting.ColumnName, ArchivedColumn, StringComparison.OrdinalIgnoreCase))
        {
            sorting.ColumnName = DeletedColumn;
        }

        return await unitOfWork.CustomerRepository.GetCustomersAsync(
            request.LegalNameFilter,
            request.VatNumberFilter,
            request.StatusFilter,
            sorting,
            request.Start,
            request.Size);
    }

    public async Task<Customer> GetCustomerByIdAsync(int idCustomer) =>
        await unitOfWork.CustomerRepository.GetByIdAsync(idCustomer)
        ?? throw new NotFoundException($"Customer {idCustomer} not found");

    public async Task<Customer> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var toCreate = CustomerMapper.ToEntity(request);
        await EnsureVatNumberIsUniqueAsync(toCreate.VatNumber, excludedIdCustomer: null);

        return await unitOfWork.CustomerRepository.CreateAsync(toCreate);
    }

    public async Task<Customer> EditCustomerAsync(EditCustomerRequest request)
    {
        var current = await GetCustomerByIdAsync(request.IdCustomer);
        if (current.Deleted)
        {
            throw new ApplicationValidationException(
                GeneralField,
                "Il cliente è archiviato: ripristinalo prima di modificarlo.");
        }

        var toEdit = CustomerMapper.ToEntity(request);
        await EnsureVatNumberIsUniqueAsync(toEdit.VatNumber, request.IdCustomer);

        return await unitOfWork.CustomerRepository.EditAsync(toEdit);
    }

    public async Task ArchiveCustomerAsync(int idCustomer)
    {
        var current = await GetCustomerByIdAsync(idCustomer);
        if (current.Deleted)
        {
            throw new ApplicationValidationException(GeneralField, "Il cliente è già archiviato.");
        }

        await unitOfWork.CustomerRepository.ArchiveAsync(idCustomer);
    }

    public async Task RestoreCustomerAsync(int idCustomer)
    {
        var current = await GetCustomerByIdAsync(idCustomer);
        if (!current.Deleted)
        {
            throw new ApplicationValidationException(GeneralField, "Il cliente non è archiviato.");
        }

        await unitOfWork.CustomerRepository.RestoreAsync(idCustomer);
    }

    // La partita IVA è unica anche tra i clienti archiviati: un cliente archiviato si ripristina, non si duplica.
    private async Task EnsureVatNumberIsUniqueAsync(string vatNumber, int? excludedIdCustomer)
    {
        if (await unitOfWork.CustomerRepository.VatNumberExistsAsync(vatNumber, excludedIdCustomer))
        {
            throw new ApplicationValidationException(
                VatNumberField,
                "Esiste già un cliente con questa partita IVA.");
        }
    }
}
