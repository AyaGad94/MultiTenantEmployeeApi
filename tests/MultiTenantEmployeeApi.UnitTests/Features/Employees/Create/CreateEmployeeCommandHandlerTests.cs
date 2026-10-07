using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Features.Employees.Create;

namespace MultiTenantEmployeeApi.UnitTests.Features.Employees.Create;

public sealed class CreateEmployeeCommandHandlerTests
{
    private static readonly Guid TenantAId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Handle_CreatesEmployeeForCurrentTenant_WhenRequestIsValid()
    {
        await using var dbContext = CreateDbContext();

        var tenantContext = CreateTenantContext(TenantAId);

        var handler = new CreateEmployeeCommandHandler(
            dbContext,
            tenantContext);

        var command = new CreateEmployeeCommand
        {
            FirstName = "Aya",
            LastName = "Gad",
            Email = "Aya.Gad@Example.com",
            Department = "Engineering",
            Status = "active"
        };

        var createResult = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(createResult.EmailAlreadyExists);
        Assert.NotNull(createResult.EmployeeId);

        var createdEmployee = await dbContext.Employees
            .SingleAsync();

        Assert.Equal(createResult.EmployeeId, createdEmployee.Id);
        Assert.Equal(TenantAId, createdEmployee.TenantId);
        Assert.Equal("Aya", createdEmployee.FirstName);
        Assert.Equal("Gad", createdEmployee.LastName);
        Assert.Equal("aya.gad@example.com", createdEmployee.Email);
        Assert.Equal("Engineering", createdEmployee.Department);
        Assert.Equal(EmployeeStatus.Active, createdEmployee.Status);
        Assert.Null(createdEmployee.DeletedAt);
    }

    [Fact]
    public async Task Handle_ReturnsDuplicateEmail_WhenEmailAlreadyExistsForCurrentTenant()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Employees.Add(
            new Employee
            {
                Id = Guid.NewGuid(),
                TenantId = TenantAId,
                FirstName = "Existing",
                LastName = "Employee",
                Email = "aya.gad@example.com",
                Department = "Engineering",
                Status = EmployeeStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var tenantContext = CreateTenantContext(TenantAId);

        var handler = new CreateEmployeeCommandHandler(
            dbContext,
            tenantContext);

        var command = new CreateEmployeeCommand
        {
            FirstName = "Aya",
            LastName = "Gad",
            Email = "AYA.GAD@EXAMPLE.COM",
            Department = "Engineering",
            Status = "active"
        };

        var createResult = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(createResult.EmailAlreadyExists);
        Assert.Null(createResult.EmployeeId);

        var employeeCount = await dbContext.Employees.CountAsync();

        Assert.Equal(1, employeeCount);
    }

    private static EmployeeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmployeeDbContext>()
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