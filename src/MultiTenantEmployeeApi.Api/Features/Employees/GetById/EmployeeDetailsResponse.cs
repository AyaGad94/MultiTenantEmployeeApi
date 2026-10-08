using System.Text.Json;
using MultiTenantEmployeeApi.Api.Common.Money;

namespace MultiTenantEmployeeApi.Api.Features.Employees.GetById;

public sealed class EmployeeDetailsResponse
{
    public Guid Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public JsonDocument? CustomData { get; init; }

    public SalaryResponse? Salary { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}