using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Features.Employees.GetById;
using MultiTenantEmployeeApi.Api.Features.Employees.List;

namespace MultiTenantEmployeeApi.UnitTests.Features.Employees.Tenancy;

public sealed class TenantIsolationTests
{
    private static readonly Guid TenantAId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid TenantBId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task ListEmployees_ReturnsOnlyEmployeesFromCurrentTenant()
    {
        var tenantContext = CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateDbContext(tenantContext);

        dbContext.Employees.AddRange(
            CreateEmployee(
                tenantId: TenantAId,
                email: "tenant-a@example.com"),
            CreateEmployee(
                tenantId: TenantBId,
                email: "tenant-b@example.com"));

        await dbContext.SaveChangesAsync();

        var handler = new ListEmployeesQueryHandler(
            dbContext);

        var query = new ListEmployeesQuery
        {
            Page = 1,
            PageSize = 20
        };

        var listResult = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.Single(listResult.Employees);

        Assert.Equal(
            "tenant-a@example.com",
            listResult.Employees[0].Email);

        Assert.Equal(
            1,
            listResult.Pagination.TotalCount);
    }

    [Fact]
    public async Task GetEmployeeById_ReturnsNull_WhenEmployeeBelongsToAnotherTenant()
    {
        var tenantContext = CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateDbContext(tenantContext);

        var tenantBEmployee = CreateEmployee(
            tenantId: TenantBId,
            email: "tenant-b@example.com");

        dbContext.Employees.Add(tenantBEmployee);

        await dbContext.SaveChangesAsync();

        var handler = new GetEmployeeByIdQueryHandler(
            dbContext);

        var query =
            new GetEmployeeByIdQuery(
                tenantBEmployee.Id);

        var employee = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.Null(employee);
    }

    private static Employee CreateEmployee(
        Guid tenantId,
        string email)
    {
        var currentUtcTime = DateTimeOffset.UtcNow;

        return new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = "Test",
            LastName = "Employee",
            Email = email,
            Department = "Engineering",
            Status = EmployeeStatus.Active,
            CreatedAt = currentUtcTime,
            UpdatedAt = currentUtcTime
        };
    }

    private static EmployeeDbContext CreateDbContext(
        ITenantContext tenantContext)
    {
        var options =
            new DbContextOptionsBuilder<EmployeeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new EmployeeDbContext(
            options,
            tenantContext);
    }

    private static TenantContext CreateTenantContext(
        Guid tenantId)
    {
        var tenantContext = new TenantContext();

        tenantContext.SetTenantId(tenantId);

        return tenantContext;
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEmployees()
    {
        var tenantContext = CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateDbContext(tenantContext);

        var activeEmployee = CreateEmployee(
            tenantId: TenantAId,
            email: "active@example.com");

        var deletedEmployee = CreateEmployee(
            tenantId: TenantAId,
            email: "deleted@example.com");

        deletedEmployee.DeletedAt =
            DateTimeOffset.UtcNow;

        dbContext.Employees.AddRange(
            activeEmployee,
            deletedEmployee);

        await dbContext.SaveChangesAsync();

        var visibleEmployees = await dbContext.Employees
            .AsNoTracking()
            .ToListAsync();

        Assert.Single(visibleEmployees);

        Assert.Equal(
            "active@example.com",
            visibleEmployees[0].Email);
    }
}