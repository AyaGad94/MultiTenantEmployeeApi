using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.IntegrationTests.Infrastructure;
using Testcontainers.PostgreSql;

namespace MultiTenantEmployeeApi.IntegrationTests.Features.Employees;

public sealed class CreateEmployeeIntegrationTests
    : IAsyncLifetime
{
    private static readonly Guid TenantAId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder("postgres:16-alpine")
            .WithImage("postgres:16-alpine")
            .WithDatabase("employee_integration_tests")
            .WithUsername("integration_user")
            .WithPassword("integration_password")
            .Build();

    private PostgreSqlWebApplicationFactory? _applicationFactory;

    private HttpClient? _httpClient;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _applicationFactory =
            new PostgreSqlWebApplicationFactory(
                _postgresContainer.GetConnectionString());

        _httpClient = _applicationFactory.CreateClient();

        using var serviceScope =
            _applicationFactory.Services.CreateScope();

        var dbContext = serviceScope.ServiceProvider
            .GetRequiredService<EmployeeDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    [Fact]
    public async Task CreateEmployee_PersistsEmployeeInPostgreSql()
    {
        var requestBody = new
        {
            firstName = "Integration",
            lastName = "Employee",
            email = "Integration.Employee@Example.com",
            department = "Engineering",
            status = "active",
            customData = new
            {
                jobLevel = "Senior",
                officeLocation = "Alexandria",
                yearsExperience = 5
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/employees");

        request.Headers.Add(
            "X-Tenant-Id",
            TenantAId.ToString());

        request.Content =
            JsonContent.Create(requestBody);

        var response = await _httpClient!.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var serviceScope =
            _applicationFactory!.Services.CreateScope();

        var tenantContext = serviceScope.ServiceProvider
            .GetRequiredService<TenantContext>();

        tenantContext.SetTenantId(TenantAId);

        var dbContext = serviceScope.ServiceProvider
            .GetRequiredService<EmployeeDbContext>();

        var createdEmployee = await dbContext.Employees
            .AsNoTracking()
            .SingleAsync(employee =>
                employee.Email ==
                "integration.employee@example.com");

        Assert.Equal(
            "Integration",
            createdEmployee.FirstName);

        Assert.Equal(
            "Employee",
            createdEmployee.LastName);

        Assert.Equal(
            "Engineering",
            createdEmployee.Department);

        Assert.Equal(
            EmployeeStatus.Active,
            createdEmployee.Status);

        Assert.Null(createdEmployee.DeletedAt);
    }

    [Fact]
    public async Task CreateEmployee_ReturnsStandardErrorEnvelope_WhenJsonIsMalformed()
    {
        const string malformedJson =
            """
            {
              "firstName": "Bad",
              "lastName":
            }
            """;

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/employees");

        request.Headers.Add(
            "X-Tenant-Id",
            TenantAId.ToString());

        request.Content = new StringContent(
            malformedJson,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient!.SendAsync(
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        using var responseDocument =
            JsonDocument.Parse(responseBody);

        var rootElement =
            responseDocument.RootElement;

        Assert.Equal(
            JsonValueKind.Null,
            rootElement.GetProperty("data").ValueKind);

        Assert.Equal(
            JsonValueKind.Null,
            rootElement.GetProperty("pagination").ValueKind);

        var errorMessage =
            rootElement.GetProperty("error").GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(errorMessage));

        Assert.False(
            rootElement.TryGetProperty(
                "type",
                out _));

        Assert.False(
            rootElement.TryGetProperty(
                "title",
                out _));
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_applicationFactory is not null)
        {
            await _applicationFactory.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();
    }
}