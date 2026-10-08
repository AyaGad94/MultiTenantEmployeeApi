using System.Text.Json;
using MultiTenantEmployeeApi.Api.ValueObjects;

namespace MultiTenantEmployeeApi.Api.Entities;

public sealed class Employee
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public EmployeeStatus Status { get; set; }

    public JsonDocument? CustomData { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public Money? Salary { get; set; }
}