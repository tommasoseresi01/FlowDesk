using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace FlowDesk.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork(
    AppDbContext context,
    IUserRepository userRepository,
    ICustomerRepository customerRepository) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IUserRepository UserRepository => userRepository;
    public ICustomerRepository CustomerRepository => customerRepository;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose() => _transaction?.Dispose();
}
