using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Options;
using FlowDesk.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(ServiceCollectionExtension));

        // Un'impostazione mancante fa fallire l'avvio con un messaggio chiaro, non la prima chiamata.
        services.AddOptions<AzureAdOption>()
            .Bind(configuration.GetSection(AzureAdOption.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}
