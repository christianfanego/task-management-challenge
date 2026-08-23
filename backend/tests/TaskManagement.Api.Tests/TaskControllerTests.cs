using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Infrastructure.Data;
using Xunit;

namespace TaskManagement.Api.Tests;

public class TaskControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly string _dbName = Guid.NewGuid().ToString();

    public TaskControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });
        }).CreateClient();
    }

    private async Task<string> RegisterAndGetToken(string email = "test@example.com", string password = "Password1!")
    {
        await _client.PostAsJsonAsync("/api/auth/register", new { Email = email, Password = password });
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.AccessToken;
    }

    private HttpClient WithAuth(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return _client;
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithToken_Returns200()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var response = await client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks!);
    }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var request = new { Title = "Test Task", Description = "Description", Status = "Pending", DueDate = (string?)null };
        var response = await client.PostAsJsonAsync("/api/tasks", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.Equal("Test Task", task!.Title);
        Assert.Equal("Pending", task.Status);
    }

    [Fact]
    public async Task GetById_ExistingTask_Returns200()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var createResponse = await client.PostAsJsonAsync("/api/tasks",
            new { Title = "Task", Description = (string?)null, Status = "Pending", DueDate = (string?)null });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await client.GetAsync($"/api/tasks/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonexistentTask_Returns404()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ForeignTask_Returns404()
    {
        var token1 = await RegisterAndGetToken("user1@example.com");
        var token2 = await RegisterAndGetToken("user2@example.com");

        using var client1 = WithAuth(token1);
        var createResponse = await client1.PostAsJsonAsync("/api/tasks",
            new { Title = "My Task", Description = (string?)null, Status = "Pending", DueDate = (string?)null });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        using var client2 = WithAuth(token2);
        var response = await client2.GetAsync($"/api/tasks/{created!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidData_Returns200()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var createResponse = await client.PostAsJsonAsync("/api/tasks",
            new { Title = "Old", Description = (string?)null, Status = "Pending", DueDate = (string?)null });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await client.PutAsJsonAsync($"/api/tasks/{created!.Id}",
            new { Title = "New", Description = "Updated", Status = "InProgress", DueDate = (string?)null });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.Equal("New", updated!.Title);
        Assert.Equal("InProgress", updated.Status);
    }

    [Fact]
    public async Task Update_NonexistentTask_Returns404()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var response = await client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}",
            new { Title = "New", Description = (string?)null, Status = "Pending", DueDate = (string?)null });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTask_Returns204()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var createResponse = await client.PostAsJsonAsync("/api/tasks",
            new { Title = "To Delete", Description = (string?)null, Status = "Pending", DueDate = (string?)null });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await client.DeleteAsync($"/api/tasks/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonexistentTask_Returns404()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutTitle_Returns400()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var response = await client.PostAsJsonAsync("/api/tasks",
            new { Title = "", Description = (string?)null, Status = "Pending", DueDate = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithDateOnly_ReturnsSameDateBack()
    {
        var token = await RegisterAndGetToken();
        using var client = WithAuth(token);
        var response = await client.PostAsJsonAsync("/api/tasks",
            new { Title = "Date Test", Description = (string?)null, Status = "Pending", DueDate = "2026-01-02" });
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(task);
        Assert.Equal(new DateTime(2026, 1, 2), task!.DueDate?.Date);
    }

    public record AuthResponse(string AccessToken, string TokenType, DateTime ExpiresAt, object User);
    public record TaskResponse(Guid Id, string Title, string? Description, string Status, DateTime? DueDate, DateTime CreatedAt, DateTime UpdatedAt);
}
