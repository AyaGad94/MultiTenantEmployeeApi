using System.Text.Json;
using MediatR;
using MultiTenantEmployeeApi.Api.Common.Money;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Update;

public sealed class UpdateEmployeeCommand
    : IRequest<UpdateEmployeeResult>
{
    public Guid EmployeeId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Department { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public JsonElement? CustomData { get; init; }

    public SalaryInput? Salary { get; init; }
}