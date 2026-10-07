using FluentValidation;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Update;

public sealed class UpdateEmployeeValidator
    : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(command => command.EmployeeId)
            .NotEmpty();

        RuleFor(command => command.FirstName)
            .NotEmpty();

        RuleFor(command => command.LastName)
            .NotEmpty();

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Department)
            .NotEmpty();

        RuleFor(command => command.Status)
            .NotEmpty()
            .Must(IsSupportedStatus)
            .WithMessage(
                "Status must be 'active' or 'suspended'.");
    }

    private static bool IsSupportedStatus(string status)
    {
        return string.Equals(
                   status,
                   "active",
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   status,
                   "suspended",
                   StringComparison.OrdinalIgnoreCase);
    }
}