using FlowDesk.Application.Models.Common;
using FlowDesk.Application.Models.Requests.Customers;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Services;

public interface ICustomerService
{
    Task<PaginatedList<Customer>> SearchCustomersAsync(CustomerSearchRequest request);
    Task<Customer> GetCustomerByIdAsync(int idCustomer);
    Task<Customer> CreateCustomerAsync(CreateCustomerRequest request);
    Task<Customer> EditCustomerAsync(EditCustomerRequest request);
    Task ArchiveCustomerAsync(int idCustomer);
    Task RestoreCustomerAsync(int idCustomer);
}
