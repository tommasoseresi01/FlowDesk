using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Infrastructure.Persistence.Context;
using FlowDesk.Infrastructure.Persistence.Repositories;
using FlowDesk.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    private const string ConnectionName = "AppDbContext";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, option) =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionName)
                ?? throw new InvalidOperationException(
                    $"Manca la connection string '{ConnectionName}': impostala con i user secrets o le variabili d'ambiente.");

            option
                .UseSqlServer(connectionString, o =>
                {
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    o.CommandTimeout(120);
                })
                .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
