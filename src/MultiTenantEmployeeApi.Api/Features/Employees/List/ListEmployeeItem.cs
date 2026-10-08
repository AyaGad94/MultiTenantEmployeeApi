using MultiTenantEmployeeApi.Api.Common.Money;

namespace MultiTenantEmployeeApi.Api.Features.Employees.List;

public sealed class ListEmployeeItem
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public SalaryResponse? Salary { get; init; }
}