using FlowDesk.Web.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace FlowDesk.Web.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication AddWebMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
        }
        else
        {
            app.UseHsts();
        }

        // Il gestore vero è ApiExceptionHandler, registrato in AddUI.
        app.UseExceptionHandler(_ => { });
        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors(ServiceCollectionExtension.CorsPolicy);
        app.UseAuthentication();
        app.UseMiddleware<LogUserNameMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        // "live" dice che il processo risponde; "ready" che il database è raggiungibile.
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") })
            .AllowAnonymous();

        return app;
    }
}
