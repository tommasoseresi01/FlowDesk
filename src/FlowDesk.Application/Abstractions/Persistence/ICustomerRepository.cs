using FlowDesk.Application.Models.Common;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task<PaginatedList<Customer>> GetCustomersAsync(
        string legalNameFilter,
        string vatNumberFilter,
        string statusFilter,
        SortingInfo? sortingInfo,
        int start,
        int size);

    Task<Customer?> GetByIdAsync(int idCustomer);
    Task<bool> VatNumberExistsAsync(string vatNumber, int? excludedIdCustomer);
    Task<Customer> CreateAsync(Customer toCreate);
    Task<Customer> EditAsync(Customer toEdit);
    Task ArchiveAsync(int idCustomer);
    Task RestoreAsync(int idCustomer);
}
