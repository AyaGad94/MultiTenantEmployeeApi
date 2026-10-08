using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Entities.Audit;
using MultiTenantEmployeeApi.Api.Features.Employees.Create;

namespace MultiTenantEmployeeApi.UnitTests.Features.Employees.Create;

public sealed class CreateEmployeeCommandHandlerTests
{
    private static readonly Guid TenantAId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Handle_CreatesEmployeeForCurrentTenant_WhenRequestIsValid()
    {
        var tenantContext = CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateDbContext(tenantContext);

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

        Assert.Equal(
            createResult.EmployeeId,
            createdEmployee.Id);

        Assert.Equal(
            TenantAId,
            createdEmployee.TenantId);

        Assert.Equal(
            "Aya",
            createdEmployee.FirstName);

        Assert.Equal(
            "Gad",
            createdEmployee.LastName);

        Assert.Equal(
            "aya.gad@example.com",
            createdEmployee.Email);

        Assert.Equal(
            "Engineering",
            createdEmployee.Department);

        Assert.Equal(
            EmployeeStatus.Active,
            createdEmployee.Status);

        Assert.Null(createdEmployee.DeletedAt);

        var auditLog = await dbContext.AuditLogs
            .SingleAsync();

        Assert.Equal(
            TenantAId,
            auditLog.TenantId);

        Assert.Equal(
            createdEmployee.Id,
            auditLog.EmployeeId);

        Assert.Equal(
            AuditAction.Created,
            auditLog.Action);

        Assert.Equal(
            createdEmployee.CreatedAt,
            auditLog.OccurredAt);
    }

    [Fact]
    public async Task Handle_ReturnsDuplicateEmail_WhenEmailAlreadyExistsForCurrentTenant()
    {
        var tenantContext = CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateDbContext(tenantContext);

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

        var employeeCount =
            await dbContext.Employees.CountAsync();

        Assert.Equal(
            1,
            employeeCount);

        var auditLogCount =
            await dbContext.AuditLogs.CountAsync();

        Assert.Equal(
            0,
            auditLogCount);
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

    private static TenantContext CreateTenantContext(Guid tenantId)
    {
        var tenantContext = new TenantContext();

        tenantContext.SetTenantId(tenantId);

        return tenantContext;
    }
}