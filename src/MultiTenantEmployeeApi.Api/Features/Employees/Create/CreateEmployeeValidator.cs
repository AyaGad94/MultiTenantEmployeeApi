using FluentValidation;
using MultiTenantEmployeeApi.Api.Common.CustomData;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Entities;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Create;

public sealed class CreateEmployeeValidator
    : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator(
        ICustomDataValidator customDataValidator,
        ITenantContext tenantContext)
    {
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
                            nameof(CreateEmployeeCommand.CustomData),
                            validationError);
                    }
                });

        When(
            command => command.Salary is not null,
            () =>
            {
                RuleFor(command =>
                        command.Salary!.AmountMinor)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage(
                        "Salary amountMinor cannot be negative.");

                RuleFor(command =>
                        command.Salary!.CurrencyCode)
                    .NotEmpty()
                    .WithMessage(
                        "Salary currencyCode is required.");
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