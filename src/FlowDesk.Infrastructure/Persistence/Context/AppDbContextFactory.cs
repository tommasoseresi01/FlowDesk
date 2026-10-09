using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlowDesk.Infrastructure.Persistence.Context;

// Usata solo dagli strumenti "dotnet ef" (migrazioni): evita di avviare l'intera applicazione.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string ConnectionVariable = "FLOWDESK_CONNECTION";
    private const string DefaultConnection =
        "Server=localhost;Database=FlowDesk;Trusted_Connection=True;TrustServerCertificate=True";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable(ConnectionVariable) ?? DefaultConnection;
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connection)
            .Options;

        return new AppDbContext(options);
    }
}
