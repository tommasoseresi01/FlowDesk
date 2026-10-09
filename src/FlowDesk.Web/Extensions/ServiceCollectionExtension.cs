using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Converters;
using FlowDesk.Infrastructure.Persistence.Context;
using FlowDesk.Web.Auth;
using FlowDesk.Web.Factories;
using FlowDesk.Web.Middlewares;
using FlowDesk.Web.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;

namespace FlowDesk.Web.Extensions;

public static class ServiceCollectionExtension
{
    public const string CorsPolicy = "CORS_POLICY";

    public static IServiceCollection AddUI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddExceptionHandler<ApiExceptionHandler>();

        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new MyDateTimeConverter()))
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = context => new BadRequestResultFactory(context));

        services.AddJwtAuthentication();

        // Nega per default: un controller senza [Authorize] non resta pubblico per dimenticanza.
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(CorsPolicy, builder => builder
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        // I validator girano prima dell'azione; le data annotation restano disattivate.
        services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotationsValidation = true);

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

        return services;
    }
}
