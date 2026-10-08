using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Entities.Audit;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Update;

public sealed class UpdateEmployeeCommandHandler
    : IRequestHandler<UpdateEmployeeCommand, UpdateEmployeeResult>
{
    private readonly EmployeeDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public UpdateEmployeeCommandHandler(
        EmployeeDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<UpdateEmployeeResult> Handle(
        UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantContext.TenantId;

        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(
                currentEmployee =>
                    currentEmployee.Id == command.EmployeeId,
                cancellationToken);

        if (employee is null)
        {
            return UpdateEmployeeResult.NotFound();
        }

        var normalizedEmail =
            command.Email.Trim().ToLowerInvariant();

        if (!string.Equals(
                employee.Email,
                normalizedEmail,
                StringComparison.Ordinal))
        {
            var emailAlreadyExists = await _dbContext.Employees
                .AnyAsync(
                    otherEmployee =>
                        otherEmployee.Id != employee.Id &&
                        otherEmployee.Email == normalizedEmail,
                    cancellationToken);

            if (emailAlreadyExists)
            {
                return UpdateEmployeeResult.DuplicateEmail();
            }
        }

        var employeeStatus = Enum.Parse<EmployeeStatus>(
            command.Status,
            ignoreCase: true);

        JsonDocument? updatedCustomData = null;

        if (command.CustomData.HasValue)
        {
            updatedCustomData = JsonDocument.Parse(
                command.CustomData.Value.GetRawText());
        }

        var previousCustomData = employee.CustomData;

        try
        {
            var currentUtcTime =
                DateTimeOffset.UtcNow;

            employee.FirstName = command.FirstName.Trim();
            employee.LastName = command.LastName.Trim();
            employee.Email = normalizedEmail;
            employee.Department = command.Department.Trim();
            employee.Status = employeeStatus;
            employee.CustomData = updatedCustomData;
            employee.UpdatedAt = currentUtcTime;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = currentTenantId,
                EmployeeId = employee.Id,
                Action = AuditAction.Updated,
                OccurredAt = currentUtcTime
            };

            _dbContext.AuditLogs.Add(auditLog);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return UpdateEmployeeResult.Updated(
                employee.Id);
        }
        finally
        {
            previousCustomData?.Dispose();
        }
    }
}