using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Entities.Audit;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Create;

public sealed class CreateEmployeeCommandHandler
    : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResult>
{
    private readonly EmployeeDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateEmployeeCommandHandler(
        EmployeeDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<CreateEmployeeResult> Handle(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantContext.TenantId;

        var normalizedEmail =
            command.Email.Trim().ToLowerInvariant();

        var emailAlreadyExists = await _dbContext.Employees
            .AnyAsync(
                employee =>
                    employee.Email == normalizedEmail,
                cancellationToken);

        if (emailAlreadyExists)
        {
            return CreateEmployeeResult.DuplicateEmail();
        }

        var employeeStatus = Enum.Parse<EmployeeStatus>(
            command.Status,
            ignoreCase: true);

        var currentUtcTime = DateTimeOffset.UtcNow;

        JsonDocument? customDataDocument = null;

        if (command.CustomData.HasValue)
        {
            customDataDocument = JsonDocument.Parse(
                command.CustomData.Value.GetRawText());
        }

        try
        {
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                TenantId = currentTenantId,
                FirstName = command.FirstName.Trim(),
                LastName = command.LastName.Trim(),
                Email = normalizedEmail,
                Department = command.Department.Trim(),
                Status = employeeStatus,
                CustomData = customDataDocument,
                CreatedAt = currentUtcTime,
                UpdatedAt = currentUtcTime,
                DeletedAt = null
            };

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = currentTenantId,
                EmployeeId = employee.Id,
                Action = AuditAction.Created,
                OccurredAt = currentUtcTime
            };

            _dbContext.Employees.Add(employee);
            _dbContext.AuditLogs.Add(auditLog);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return CreateEmployeeResult.Created(
                employee.Id);
        }
        finally
        {
            customDataDocument?.Dispose();
        }
    }
}