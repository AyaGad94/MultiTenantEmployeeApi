using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Features.Employees.List;

namespace MultiTenantEmployeeApi.UnitTests.Features.Employees.List;

public sealed class ListEmployeesQueryHandlerTests
{
    private static readonly Guid TenantAId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Handle_ReturnsRequestedPageWithPaginationMetadata()
    {
        await using var dbContext = CreateDbContext();

        var firstCreatedAt =
            new DateTimeOffset(
                2026,
                10,
                7,
                12,
                0,
                0,
                TimeSpan.Zero);

        for (var employeeNumber = 1;
             employeeNumber <= 5;
             employeeNumber++)
        {
            dbContext.Employees.Add(
                new Employee
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantAId,
                    FirstName = $"Employee{employeeNumber}",
                    LastName = "Test",
                    Email =
                        $"employee{employeeNumber}@example.com",
                    Department = "Engineering",
                    Status = EmployeeStatus.Active,
                    CreatedAt =
                        firstCreatedAt.AddMinutes(employeeNumber),
                    UpdatedAt =
                        firstCreatedAt.AddMinutes(employeeNumber)
                });
        }

        await dbContext.SaveChangesAsync();

        var tenantContext = CreateTenantContext(TenantAId);

        var handler = new ListEmployeesQueryHandler(
            dbContext,
            tenantContext);

        var query = new ListEmployeesQuery
        {
            Page = 2,
            PageSize = 2
        };

        var listResult = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.Equal(2, listResult.Employees.Count);

        Assert.Equal(
            "employee3@example.com",
            listResult.Employees[0].Email);

        Assert.Equal(
            "employee4@example.com",
            listResult.Employees[1].Email);

        Assert.Equal(2, listResult.Pagination.Page);
        Assert.Equal(2, listResult.Pagination.PageSize);
        Assert.Equal(5, listResult.Pagination.TotalCount);
        Assert.Equal(3, listResult.Pagination.TotalPages);
    }

    private static EmployeeDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<EmployeeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new EmployeeDbContext(options);
    }

    private static TenantContext CreateTenantContext(Guid tenantId)
    {
        var tenantContext = new TenantContext();

        tenantContext.SetTenantId(tenantId);

        return tenantContext;
    }
}