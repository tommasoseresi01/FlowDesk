using FlowDesk.Application.Extensions;
using FlowDesk.Infrastructure.Extensions;
using FlowDesk.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.InitSerilogFromConfiguration();

builder.Services
    .AddUI(builder.Configuration)              // Web
    .AddApplication(builder.Configuration)     // Application
    .AddInfrastructure(builder.Configuration); // Infrastructure

var app = builder.Build();

app.AddWebMiddleware()
   .Run();

// Rende Program visibile ai test di integrazione (WebApplicationFactory).
public partial class Program;
