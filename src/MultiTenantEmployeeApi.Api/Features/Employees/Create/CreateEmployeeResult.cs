namespace MultiTenantEmployeeApi.Api.Features.Employees.Create;

public sealed class CreateEmployeeResult
{
    public Guid? EmployeeId { get; init; }

    public bool EmailAlreadyExists { get; init; }

    public static CreateEmployeeResult Created(Guid employeeId)
    {
        return new CreateEmployeeResult
        {
            EmployeeId = employeeId,
            EmailAlreadyExists = false
        };
    }

    public static CreateEmployeeResult DuplicateEmail()
    {
        return new CreateEmployeeResult
        {
            EmployeeId = null,
            EmailAlreadyExists = true
        };
    }
}