using System.Reflection;
using FlowDesk.Application;
using FlowDesk.Domain;
using FlowDesk.Infrastructure;
using NetArchTest.Rules;

namespace FlowDesk.Architecture.Tests;

public class LayerDependencyTests
{
    private const string Application = "FlowDesk.Application";
    private const string Infrastructure = "FlowDesk.Infrastructure";
    private const string Api = "FlowDesk.Api";

    [Fact]
    public void Domain_does_not_depend_on_any_other_layer()
    {
        AssertNoDependency(DomainAssembly.Instance, Application, Infrastructure, Api);
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_or_api()
    {
        AssertNoDependency(ApplicationAssembly.Instance, Infrastructure, Api);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_api()
    {
        AssertNoDependency(InfrastructureAssembly.Instance, Api);
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
