using TaskManagement.Application.Ports;
using Xunit;

namespace TaskManagement.Application.Tests;

public class PortInterfaceTests
{
    [Fact]
    public void ITaskRepository_InterfaceExists()
    {
        Assert.True(typeof(ITaskRepository).IsInterface);
    }

    [Fact]
    public void IUserRepository_InterfaceExists()
    {
        Assert.True(typeof(IUserRepository).IsInterface);
    }

    [Fact]
    public void IPasswordHasher_InterfaceExists()
    {
        Assert.True(typeof(IPasswordHasher).IsInterface);
    }

    [Fact]
    public void IJwtTokenGenerator_InterfaceExists()
    {
        Assert.True(typeof(IJwtTokenGenerator).IsInterface);
    }
}
