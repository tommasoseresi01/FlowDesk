namespace FlowDesk.Application.Abstractions.Persistence;

// Punto di accesso ai repository e gestore delle transazioni.
// Il salvataggio lo fa il repository; l'audit lo valorizza l'interceptor del DbContext.
public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }
    ICustomerRepository CustomerRepository { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
