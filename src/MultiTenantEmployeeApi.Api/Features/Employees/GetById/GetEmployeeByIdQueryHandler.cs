using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Entities;

namespace MultiTenantEmployeeApi.Api.Features.Employees.GetById;

public sealed class GetEmployeeByIdQueryHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDetailsResponse?>
{
    private readonly EmployeeDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetEmployeeByIdQueryHandler(
        EmployeeDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<EmployeeDetailsResponse?> Handle(
        GetEmployeeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var currentTenantId = _tenantContext.TenantId;

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.Id == query.EmployeeId &&
                employee.TenantId == currentTenantId &&
                employee.DeletedAt == null)
            .Select(employee => new EmployeeDetailsResponse
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Department = employee.Department,
                Status =
                    employee.Status == EmployeeStatus.Active
                        ? "active"
                        : "suspended",
                CustomData = employee.CustomData,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}