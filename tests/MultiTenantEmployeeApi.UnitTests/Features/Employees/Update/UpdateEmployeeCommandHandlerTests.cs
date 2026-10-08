using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Entities.Audit;
using MultiTenantEmployeeApi.Api.Features.Employees.Update;

namespace MultiTenantEmployeeApi.UnitTests.Features.Employees.Update;

public sealed class UpdateEmployeeCommandHandlerTests
{
    private static readonly Guid TenantAId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Handle_UpdatesEmployeeAndCreatesAuditLog_WhenRequestIsValid()
    {
        var tenantContext =
            CreateTenantContext(TenantAId);

        await using var dbContext =
            CreateDbContext(tenantContext);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = TenantAId,
            FirstName = "Aya",
            LastName = "Gad",
            Email = "aya.gad@example.com",
            Department = "Engineering",
            Status = EmployeeStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            DeletedAt = null
        };

        dbContext.Employees.Add(employee);

        await dbContext.SaveChangesAsync();

        var handler =
            new UpdateEmployeeCommandHandler(
                dbContext,
                tenantContext);

        var command = new UpdateEmployeeCommand
        {
            EmployeeId = employee.Id,
            FirstName = "Aya",
            LastName = "Updated",
            Email = "Aya.Updated@Example.com",
            Department = "Platform Engineering",
            Status = "suspended"
        };

        var updateResult = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(updateResult.EmployeeId);

        var updatedEmployee =
            await dbContext.Employees.SingleAsync();

        Assert.Equal(
            employee.Id,
            updatedEmployee.Id);

        Assert.Equal(
            "Updated",
            updatedEmployee.LastName);

        Assert.Equal(
            "aya.updated@example.com",
            updatedEmployee.Email);

        Assert.Equal(
            "Platform Engineering",
            updatedEmployee.Department);

        Assert.Equal(
            EmployeeStatus.Suspended,
            updatedEmployee.Status);

        var auditLog =
            await dbContext.AuditLogs.SingleAsync();

        Assert.Equal(
            TenantAId,
            auditLog.TenantId);

        Assert.Equal(
            updatedEmployee.Id,
            auditLog.EmployeeId);

        Assert.Equal(
            AuditAction.Updated,
            auditLog.Action);

        Assert.Equal(
            updatedEmployee.UpdatedAt,
            auditLog.OccurredAt);
    }

    private static EmployeeDbContext CreateDbContext(
        ITenantContext tenantContext)
    {
        var options =
            new DbContextOptionsBuilder<EmployeeDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new EmployeeDbContext(
            options,
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
}