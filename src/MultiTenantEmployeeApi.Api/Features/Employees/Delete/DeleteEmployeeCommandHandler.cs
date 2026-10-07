using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Delete;

public sealed class DeleteEmployeeCommandHandler
    : IRequestHandler<DeleteEmployeeCommand, Guid?>
{
    private readonly EmployeeDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteEmployeeCommandHandler(
        EmployeeDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Guid?> Handle(
        DeleteEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantContext.TenantId;

        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(
                currentEmployee =>
                    currentEmployee.Id == command.EmployeeId &&
                    currentEmployee.TenantId == currentTenantId &&
                    currentEmployee.DeletedAt == null,
                cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var currentUtcTime = DateTimeOffset.UtcNow;

        employee.DeletedAt = currentUtcTime;
        employee.UpdatedAt = currentUtcTime;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}