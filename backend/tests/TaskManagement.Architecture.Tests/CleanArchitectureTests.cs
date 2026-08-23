using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace TaskManagement.Architecture.Tests;

public class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = Assembly.Load("TaskManagement.Domain");
    private static readonly Assembly ApplicationAssembly = Assembly.Load("TaskManagement.Application");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load("TaskManagement.Infrastructure");
    private static readonly Assembly ApiAssembly = Assembly.Load("TaskManagement.Api");

    [Fact]
    public void Domain_Should_NotReference_Application_Infrastructure_or_Api()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TaskManagement.Application",
                "TaskManagement.Infrastructure",
                "TaskManagement.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain must not reference outer layers. Violations: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Application_Should_NotReference_Infrastructure_or_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "TaskManagement.Infrastructure",
                "TaskManagement.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application must not reference Infrastructure or Api. Violations: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Infrastructure_Should_NotReference_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("TaskManagement.Api")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Infrastructure must not reference Api. Violations: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Controllers_Should_Not_Directly_Reference_Infrastructure_Types()
    {
        var controllerTypes = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .GetTypes();

        foreach (var controller in controllerTypes)
        {
            var hasInfraTypeDependency = controller.GetFields()
                .Any(f => f.FieldType.Namespace?.StartsWith("TaskManagement.Infrastructure") == true);

            Assert.False(hasInfraTypeDependency,
                $"Controller {controller.Name} must not have direct Infrastructure type dependencies");
        }
    }
}
