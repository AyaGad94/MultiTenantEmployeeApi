namespace MultiTenantEmployeeApi.Api.Features.Employees.Update;

public sealed class UpdateEmployeeResult
{
    public Guid? EmployeeId { get; init; }

    public bool EmployeeNotFound { get; init; }

    public bool EmailAlreadyExists { get; init; }

    public static UpdateEmployeeResult Updated(Guid employeeId)
    {
        return new UpdateEmployeeResult
        {
            EmployeeId = employeeId
        };
    }

    public static UpdateEmployeeResult NotFound()
    {
        return new UpdateEmployeeResult
        {
            EmployeeNotFound = true
        };
    }

    public static UpdateEmployeeResult DuplicateEmail()
    {
        return new UpdateEmployeeResult
        {
            EmailAlreadyExists = true
        };
    }
}