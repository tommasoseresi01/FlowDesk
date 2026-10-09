using System.Reflection;
using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Persistence.Context;
using NetArchTest.Rules;

namespace FlowDesk.Architecture.Tests;

// La regola di dipendenza: Web -> Application -> Domain e Infrastructure -> Application -> Domain.
public class LayerDependencyTests
{
    private const string Application = "FlowDesk.Application";
    private const string Infrastructure = "FlowDesk.Infrastructure";
    private const string Web = "FlowDesk.Web";
    private const string EntityFramework = "Microsoft.EntityFrameworkCore";

    private static readonly Assembly DomainAssembly = typeof(Customer).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ICustomerService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(AppDbContext).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_any_other_layer()
    {
        AssertNoDependency(DomainAssembly, Application, Infrastructure, Web);
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_or_web()
    {
        AssertNoDependency(ApplicationAssembly, Infrastructure, Web);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_web()
    {
        AssertNoDependency(InfrastructureAssembly, Web);
    }

    [Fact]
    public void Domain_and_Application_do_not_know_entity_framework()
    {
        AssertNoDependency(DomainAssembly, EntityFramework);
        AssertNoDependency(ApplicationAssembly, EntityFramework);
    }

    private static void AssertNoDependency(Assembly assembly, params string[] forbiddenNamespaces)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Forbidden dependencies in: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
