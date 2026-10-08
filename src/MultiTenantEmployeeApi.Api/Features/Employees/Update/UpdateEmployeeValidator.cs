using FluentValidation;
using MultiTenantEmployeeApi.Api.Common.CustomData;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Entities;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Update;

public sealed class UpdateEmployeeValidator
    : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator(
        ICustomDataValidator customDataValidator,
        ITenantContext tenantContext)
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
            .Must(BeValidEmployeeStatus)
            .WithMessage(
                "Status must be either 'active' or 'suspended'.");

        RuleFor(command => command.CustomData)
            .Custom(
                (customData, validationContext) =>
                {
                    var currentTenantId =
                        tenantContext.TenantId;

                    var validationErrors =
                        customDataValidator.Validate(
                            currentTenantId,
                            customData);

                    foreach (var validationError in validationErrors)
                    {
                        validationContext.AddFailure(
                            nameof(UpdateEmployeeCommand.CustomData),
                            validationError);
                    }
                });
    }

    private static bool BeValidEmployeeStatus(
        string status)
    {
        return Enum.TryParse<EmployeeStatus>(
                   status,
                   ignoreCase: true,
                   out var parsedStatus)
               &&
               parsedStatus is
                   EmployeeStatus.Active or
                   EmployeeStatus.Suspended;
    }
}