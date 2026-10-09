using FlowDesk.Application.Models.Dtos;

namespace FlowDesk.Application.Models.Responses.Customers;

public class SearchCustomerResponse : PaginatedResponse<IEnumerable<CustomerDto>>
{
}

public class GetCustomerResponse : BaseResponse<CustomerDto>
{
}

public class CreateCustomerResponse : BaseResponse<CustomerDto>
{
}

public class EditCustomerResponse : BaseResponse<CustomerDto>
{
}

public class ArchiveCustomerResponse : BaseResponse<bool>
{
}

public class RestoreCustomerResponse : BaseResponse<bool>
{
}
