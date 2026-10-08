using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Data;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Delete;

public sealed class DeleteEmployeeCommandHandler
    : IRequestHandler<DeleteEmployeeCommand, Guid?>
{
    private readonly EmployeeDbContext _dbContext;

    public DeleteEmployeeCommandHandler(
        EmployeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> Handle(
        DeleteEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(
                currentEmployee =>
                    currentEmployee.Id == command.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var currentUtcTime = DateTimeOffset.UtcNow;

        employee.DeletedAt = currentUtcTime;
        employee.UpdatedAt = currentUtcTime;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return employee.Id;
    }
}