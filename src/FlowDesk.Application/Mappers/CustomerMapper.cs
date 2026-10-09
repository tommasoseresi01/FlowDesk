using FlowDesk.Application.Models.Dtos;
using FlowDesk.Application.Models.Requests.Customers;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(Customer entity)
    {
        var dto = new CustomerDto
        {
            IdCustomer = entity.IdCustomer,
            LegalName = entity.LegalName,
            VatNumber = entity.VatNumber,
            ContactName = entity.ContactName,
            Email = entity.Email,
            Phone = entity.Phone,
            Archived = entity.Deleted,
            DateCreation = entity.DateCreation,
            DateModification = entity.DateModification
        };
        dto.SetAuditableUsers(entity.UserCreation, entity.UserModification);
        return dto;
    }

    public static Customer ToEntity(CreateCustomerRequest request) => new()
    {
        LegalName = request.LegalName.Trim(),
        VatNumber = request.VatNumber.Trim(),
        ContactName = request.ContactName.Trim(),
        Email = request.Email.Trim(),
        Phone = request.Phone.Trim()
    };

    public static Customer ToEntity(EditCustomerRequest request) => new()
    {
        IdCustomer = request.IdCustomer,
        LegalName = request.LegalName.Trim(),
        VatNumber = request.VatNumber.Trim(),
        ContactName = request.ContactName.Trim(),
        Email = request.Email.Trim(),
        Phone = request.Phone.Trim()
    };
}
