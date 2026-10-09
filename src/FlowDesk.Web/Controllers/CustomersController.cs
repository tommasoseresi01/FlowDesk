using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Mappers;
using FlowDesk.Application.Models.Requests.Customers;
using FlowDesk.Application.Models.Responses.Customers;
using FlowDesk.Application.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Web.Controllers;

// Clienti: il responsabile e l'operatore lavorano sui dati; solo il responsabile archivia e ripristina.
// L'amministratore configura il sistema e non vede i clienti.
[Route("api/[controller]")]
[Route("api/v1.0/[controller]")]
[ApiController]
[Authorize(Roles = RoleNames.MANAGER_OR_OPERATOR)]
public class CustomersController(ICustomerService customerService) : Controller
{
    [HttpPost("search")]
    public async Task<IActionResult> Search(CustomerSearchRequest request)
    {
        var items = await customerService.SearchCustomersAsync(request);

        return Ok(new SearchCustomerResponse()
            .SetTotResultNumber(items.TotNum)
            .SetResult(items.Select(CustomerMapper.ToDto).ToList())
            .WithSuccess());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var customer = await customerService.GetCustomerByIdAsync(id);

        return Ok(new GetCustomerResponse()
            .SetResult(CustomerMapper.ToDto(customer))
            .WithSuccess());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        var created = await customerService.CreateCustomerAsync(request);

        return Ok(new CreateCustomerResponse()
            .SetResult(CustomerMapper.ToDto(created))
            .WithSuccess());
    }

    [HttpPut]
    public async Task<IActionResult> Edit(EditCustomerRequest request)
    {
        var edited = await customerService.EditCustomerAsync(request);

        return Ok(new EditCustomerResponse()
            .SetResult(CustomerMapper.ToDto(edited))
            .WithSuccess());
    }

    // DELETE archivia il cliente: nessun dato viene cancellato.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.MANAGER)]
    public async Task<IActionResult> Archive(int id)
    {
        await customerService.ArchiveCustomerAsync(id);

        return Ok(new ArchiveCustomerResponse().SetResult(true).WithSuccess());
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Roles = RoleNames.MANAGER)]
    public async Task<IActionResult> Restore(int id)
    {
        await customerService.RestoreCustomerAsync(id);

        return Ok(new RestoreCustomerResponse().SetResult(true).WithSuccess());
    }
}
