using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.IntegrationTests.Infrastructure;
using Testcontainers.PostgreSql;
using MultiTenantEmployeeApi.Api.Common.Tenancy;

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
                level = "senior"
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