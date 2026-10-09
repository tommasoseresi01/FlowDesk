using Serilog;

namespace FlowDesk.Web.Extensions;

public static class HostExtension
{
    // Serilog è configurato interamente da appsettings (sezione "Serilog").
    public static IHostBuilder InitSerilogFromConfiguration(this IHostBuilder hostBuilder) =>
        hostBuilder.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());
}
