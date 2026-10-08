using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Responses;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Common.Money;
using MultiTenantEmployeeApi.Api.Data;

namespace MultiTenantEmployeeApi.Api.Features.Employees.List;

public sealed class ListEmployeesQueryHandler
    : IRequestHandler<ListEmployeesQuery, ListEmployeesResult>
{
    private readonly EmployeeDbContext _dbContext;

    public ListEmployeesQueryHandler(
        EmployeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListEmployeesResult> Handle(
        ListEmployeesQuery query,
        CancellationToken cancellationToken)
    {
        var employeeQuery = _dbContext.Employees
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var requestedDepartment = query.Department.Trim();

            employeeQuery = employeeQuery.Where(
                employee =>
                    employee.Department == requestedDepartment);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var requestedStatus = Enum.Parse<EmployeeStatus>(
                query.Status.Trim(),
                ignoreCase: true);

            employeeQuery = employeeQuery.Where(
                employee =>
                    employee.Status == requestedStatus);
        }

        var totalCount = await employeeQuery.CountAsync(
            cancellationToken);

        var recordsToSkip =
            (long)(query.Page - 1) * query.PageSize;

        IReadOnlyList<ListEmployeeItem> employees;

        if (recordsToSkip >= totalCount)
        {
            employees = Array.Empty<ListEmployeeItem>();
        }
        else
        {
            employees = await employeeQuery
                .OrderBy(employee => employee.CreatedAt)
                .ThenBy(employee => employee.Id)
                .Skip((int)recordsToSkip)
                .Take(query.PageSize)
                .Select(employee => new ListEmployeeItem
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
                    Salary = employee.Salary == null
                        ? null
                        : new SalaryResponse
                        {
                            AmountMinor =
                                employee.Salary.AmountMinor,
                            CurrencyCode =
                                employee.Salary.CurrencyCode
                        }
                })
                .ToListAsync(cancellationToken);
        }

        var totalPages = totalCount == 0
            ? 0
            : (int)(((long)totalCount + query.PageSize - 1)
                    / query.PageSize);

        return new ListEmployeesResult
        {
            Employees = employees,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            }
        };
    }
}