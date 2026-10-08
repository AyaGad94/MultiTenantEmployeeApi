using System.Text.Json;
using MultiTenantEmployeeApi.Api.Common.Money;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Update;

public sealed class UpdateEmployeeRequest
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public JsonElement? CustomData { get; init; }

    public SalaryInput? Salary { get; init; }
}