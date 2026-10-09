using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Exceptions;
using FlowDesk.Application.Models.Common;
using FlowDesk.Application.Models.Requests.Customers;
using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Extensions;
using FlowDesk.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public class CustomerRepository(AppDbContext context) : ICustomerRepository
{
    private const int MaxPageSize = 200;
    private const string DuplicateVatNumberMessage = "Esiste già un cliente con questa partita IVA.";

    public async Task<PaginatedList<Customer>> GetCustomersAsync(
        string legalNameFilter,
        string vatNumberFilter,
        string statusFilter,
        SortingInfo? sortingInfo,
        int start,
        int size)
    {
        var query = context.Customers
            .AsNoTracking()
            .Include(c => c.UserCreation)
            .Include(c => c.UserModification)
            .AsQueryable();

        query = statusFilter switch
        {
            CustomerStatusFilter.ACTIVE => query.Where(c => !c.Deleted),
            CustomerStatusFilter.ARCHIVED => query.Where(c => c.Deleted),
            _ => query
        };

        if (!string.IsNullOrEmpty(legalNameFilter))
        {
            query = query.Where(c => c.LegalName.Contains(legalNameFilter));
        }

        if (!string.IsNullOrEmpty(vatNumberFilter))
        {
            query = query.Where(c => c.VatNumber.Contains(vatNumberFilter));
        }

        var result = new PaginatedList<Customer> { TotNum = await query.CountAsync() };

        // La chiave come ultimo criterio rende stabile l'ordine tra una pagina e la successiva.
        var ordered = (query.OrderByColumn(sortingInfo) ?? query.OrderBy(c => c.LegalName))
            .ThenBy(c => c.IdCustomer);

        result.AddRange(await ordered
            .Skip(Math.Max(start, 0))
            .Take(Math.Clamp(size, 1, MaxPageSize))
            .ToListAsync());
        return result;
    }

    public async Task<Customer?> GetByIdAsync(int idCustomer) =>
        await context.Customers
            .AsNoTracking()
            .Include(c => c.UserCreation)
            .Include(c => c.UserModification)
            .FirstOrDefaultAsync(c => c.IdCustomer == idCustomer);

    public async Task<bool> VatNumberExistsAsync(string vatNumber, int? excludedIdCustomer) =>
        await context.Customers.AnyAsync(c =>
            c.VatNumber == vatNumber
            && (excludedIdCustomer == null || c.IdCustomer != excludedIdCustomer));

    public async Task<Customer> CreateAsync(Customer toCreate)
    {
        await context.Customers.AddAsync(toCreate);
        await SaveChangesAsync();

        return (await GetByIdAsync(toCreate.IdCustomer))!;
    }

    public async Task<Customer> EditAsync(Customer toEdit)
    {
        var customer = await context.Customers.FirstAsync(c => c.IdCustomer == toEdit.IdCustomer);
        customer.LegalName = toEdit.LegalName;
        customer.VatNumber = toEdit.VatNumber;
        customer.ContactName = toEdit.ContactName;
        customer.Email = toEdit.Email;
        customer.Phone = toEdit.Phone;
        await SaveChangesAsync();

        return (await GetByIdAsync(toEdit.IdCustomer))!;
    }

    public async Task ArchiveAsync(int idCustomer)
    {
        var customer = await context.Customers.FirstAsync(c => c.IdCustomer == idCustomer);
        customer.Deleted = true;

        // L'interceptor registra chi ha archiviato e quando.
        await context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int idCustomer)
    {
        var customer = await context.Customers.FirstAsync(c => c.IdCustomer == idCustomer);
        customer.Deleted = false;
        customer.DateDeleted = null;
        customer.IdUserDeleted = null;

        await context.SaveChangesAsync();
    }

    // Se due richieste inseriscono la stessa partita IVA insieme, la seconda trova l'indice unico.
    private async Task SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            throw new ApplicationValidationException("VatNumber", DuplicateVatNumberMessage);
        }
    }
}
