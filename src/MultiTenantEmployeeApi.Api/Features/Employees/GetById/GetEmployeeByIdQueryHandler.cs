using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Data;

namespace MultiTenantEmployeeApi.Api.Features.Employees.GetById;

public sealed class GetEmployeeByIdQueryHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDetailsResponse?>
{
    private readonly EmployeeDbContext _dbContext;

    public GetEmployeeByIdQueryHandler(
        EmployeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EmployeeDetailsResponse?> Handle(
        GetEmployeeByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee =>
                employee.Id == query.EmployeeId)
            .Select(employee => new EmployeeDetailsResponse
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Department = employee.Department,
                Status =
                    employee.Status ==
                    Entities.EmployeeStatus.Active
                        ? "active"
                        : "suspended",
                CustomData = employee.CustomData,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}