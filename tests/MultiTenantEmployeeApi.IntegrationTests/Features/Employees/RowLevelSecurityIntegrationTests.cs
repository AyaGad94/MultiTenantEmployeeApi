using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Data.Interceptors;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.IntegrationTests.Infrastructure;
using Npgsql;
using Testcontainers.PostgreSql;

namespace MultiTenantEmployeeApi.IntegrationTests.Features.Employees;

public sealed class RowLevelSecurityIntegrationTests
    : IAsyncLifetime
{
    private static readonly Guid TenantAId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

    private static readonly Guid TenantBId =
        Guid.Parse(
            "22222222-2222-2222-2222-222222222222");

    private const string RestrictedUserName =
        "employee_app";

    private const string RestrictedUserPassword =
        "integration_app_password";

    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("employee_rls_tests")
            .WithUsername("integration_admin")
            .WithPassword("integration_admin_password")
            .Build();

    private PostgreSqlWebApplicationFactory? _applicationFactory;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _applicationFactory =
            new PostgreSqlWebApplicationFactory(
                _postgresContainer.GetConnectionString());

        using var serviceScope =
            _applicationFactory.Services.CreateScope();

        var dbContext = serviceScope.ServiceProvider
            .GetRequiredService<EmployeeDbContext>();

        await dbContext.Database.MigrateAsync();

        await CreateRestrictedDatabaseUserAsync(
            dbContext);
    }

    [Fact]
    public async Task IgnoreQueryFilters_ReturnsOnlyCurrentTenant_WhenPostgreSqlRlsIsEnabled()
    {
        var tenantAEmployeeId = Guid.NewGuid();
        var tenantBEmployeeId = Guid.NewGuid();

        await SeedEmployeeAsync(
            tenantId: TenantAId,
            employeeId: tenantAEmployeeId,
            email: "tenant-a-rls@example.com");

        await SeedEmployeeAsync(
            tenantId: TenantBId,
            employeeId: tenantBEmployeeId,
            email: "tenant-b-rls@example.com");

        var tenantContext =
            CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateRestrictedDbContext(
                tenantContext);

        var visibleEmployees = await dbContext.Employees
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(employee => new
            {
                employee.Id,
                employee.TenantId,
                employee.Email
            })
            .ToListAsync();

        Assert.Single(visibleEmployees);

        var visibleEmployee =
            visibleEmployees[0];

        Assert.Equal(
            tenantAEmployeeId,
            visibleEmployee.Id);

        Assert.Equal(
            TenantAId,
            visibleEmployee.TenantId);

        Assert.Equal(
            "tenant-a-rls@example.com",
            visibleEmployee.Email);

        Assert.DoesNotContain(
            visibleEmployees,
            employee =>
                employee.Id == tenantBEmployeeId);
    }

    private async Task SeedEmployeeAsync(
        Guid tenantId,
        Guid employeeId,
        string email)
    {
        using var serviceScope =
            _applicationFactory!.Services.CreateScope();

        var tenantContext = serviceScope.ServiceProvider
            .GetRequiredService<TenantContext>();

        tenantContext.SetTenantId(tenantId);

        var dbContext = serviceScope.ServiceProvider
            .GetRequiredService<EmployeeDbContext>();

        var currentUtcTime =
            DateTimeOffset.UtcNow;

        dbContext.Employees.Add(
            new Employee
            {
                Id = employeeId,
                TenantId = tenantId,
                FirstName = "Rls",
                LastName = "Employee",
                Email = email,
                Department = "Engineering",
                Status = EmployeeStatus.Active,
                CreatedAt = currentUtcTime,
                UpdatedAt = currentUtcTime,
                DeletedAt = null
            });

        await dbContext.SaveChangesAsync();
    }

    private static async Task CreateRestrictedDatabaseUserAsync(
        EmployeeDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_roles
                    WHERE rolname = 'employee_app'
                ) THEN
                    CREATE ROLE employee_app
                    LOGIN
                    PASSWORD 'integration_app_password'
                    NOSUPERUSER
                    NOBYPASSRLS;
                END IF;
            END
            $$;

            GRANT CONNECT
            ON DATABASE employee_rls_tests
            TO employee_app;

            GRANT USAGE
            ON SCHEMA public
            TO employee_app;

            GRANT SELECT, INSERT, UPDATE, DELETE
            ON TABLE employees
            TO employee_app;
            """);
    }

    private EmployeeDbContext CreateRestrictedDbContext(
        ITenantContext tenantContext)
    {
        var connectionStringBuilder =
            new NpgsqlConnectionStringBuilder(
                _postgresContainer.GetConnectionString())
            {
                Username = RestrictedUserName,
                Password = RestrictedUserPassword
            };

        var tenantSessionInterceptor =
            new TenantSessionConnectionInterceptor(
                tenantContext);

        var dbContextOptions =
            new DbContextOptionsBuilder<EmployeeDbContext>()
                .UseNpgsql(
                    connectionStringBuilder.ConnectionString)
                .AddInterceptors(
                    tenantSessionInterceptor)
                .Options;

        return new EmployeeDbContext(
            dbContextOptions,
            tenantContext);
    }

    private static TenantContext CreateTenantContext(
        Guid tenantId)
    {
        var tenantContext =
            new TenantContext();

        tenantContext.SetTenantId(
            tenantId);

        return tenantContext;
    }

    public async Task DisposeAsync()
    {
        if (_applicationFactory is not null)
        {
            await _applicationFactory.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task TenantACannotInsertEmployeeForTenantB_WhenPostgreSqlRlsIsEnabled()
    {
        var tenantContext =
            CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateRestrictedDbContext(
                tenantContext);

        var currentUtcTime =
            DateTimeOffset.UtcNow;

        dbContext.Employees.Add(
            new Employee
            {
                Id = Guid.NewGuid(),

                // Intentionally wrong:
                // PostgreSQL session = Tenant A
                // Employee row = Tenant B
                TenantId = TenantBId,

                FirstName = "Blocked",
                LastName = "Employee",
                Email = "blocked-cross-tenant@example.com",
                Department = "Engineering",
                Status = EmployeeStatus.Active,
                CreatedAt = currentUtcTime,
                UpdatedAt = currentUtcTime,
                DeletedAt = null
            });

        var exception =
            await Assert.ThrowsAsync<DbUpdateException>(
                () => dbContext.SaveChangesAsync());

        var postgresException =
            Assert.IsType<PostgresException>(
                exception.InnerException);

        Assert.Equal(
            PostgresErrorCodes.InsufficientPrivilege,
            postgresException.SqlState);

        Assert.True(
            postgresException.MessageText.Contains(
                "row-level security policy",
                StringComparison.OrdinalIgnoreCase));
    }
}