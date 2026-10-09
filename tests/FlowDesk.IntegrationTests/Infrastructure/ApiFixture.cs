using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Entities.Enums;
using FlowDesk.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.IntegrationTests.Infrastructure;

// Avvia l'API vera contro un SQL Server vero (un database dedicato ai test, ricreato a ogni esecuzione).
// Un solo avvio per tutti i test: le classi condividono la fixture tramite la collection.
public sealed class ApiFixture : IAsyncLifetime
{
    private const string ConnectionVariable = "FLOWDESK_TEST_CONNECTION";
    private const string DefaultConnection =
        "Server=localhost;Database=FlowDesk_Test;Trusted_Connection=True;TrustServerCertificate=True";

    private WebApplicationFactory<Program> _factory = null!;
    private readonly Dictionary<RoleEnum, int> _userIds = [];

    public IServiceProvider Services => _factory.Services;

    public int UserId(RoleEnum role) => _userIds[role];

    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable(ConnectionVariable) ?? DefaultConnection;

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:AppDbContext"] = connection,
                    ["AzureAd:TenantId"] = "00000000-0000-0000-0000-000000000001",
                    ["AzureAd:ClientId"] = "00000000-0000-0000-0000-000000000002"
                }));
            builder.ConfigureTestServices(services => services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { }));
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await RecreateDatabaseAsync(context);

        foreach (var role in Enum.GetValues<RoleEnum>())
        {
            var user = new ApplicationUser
            {
                EntraObjectId = Guid.NewGuid(),
                Email = $"{role.ToString().ToLowerInvariant()}@test.example",
                Name = role.ToString(),
                Surname = "Test",
                IdRole = role,
                Enabled = true,
                DateCreation = DateTime.UtcNow,
                DateLastLogin = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            _userIds[role] = user.IdUser;
        }
    }

    // Un SQL Server appena avviato (ad esempio in un container della CI) accetta le connessioni
    // qualche secondo prima di essere pronto: si riprova invece di far fallire tutti i test.
    private static async Task RecreateDatabaseAsync(AppDbContext context)
    {
        const int maxAttempts = 20;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.MigrateAsync();
                return;
            }
            catch (SqlException) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    // Un client che si presenta con il ruolo indicato; senza ruolo la richiesta è anonima.
    public HttpClient CreateClient(RoleEnum? role = null)
    {
        var client = _factory.CreateClient();
        if (role is { } value)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, UserId(value).ToString());
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, value.ToString());
            client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, $"{value.ToString().ToLowerInvariant()}@test.example");
        }

        return client;
    }

    // Per verificare cosa è stato scritto davvero nel database.
    public async Task<T> QueryDatabaseAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
