using FluentValidation;

namespace MultiTenantEmployeeApi.Api.Features.Employees.List;

public sealed class ListEmployeesQueryValidator
    : AbstractValidator<ListEmployeesQuery>
{
    public ListEmployeesQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Status)
            .Must(IsSupportedStatus)
            .When(query => !string.IsNullOrWhiteSpace(query.Status))
            .WithMessage("Status must be 'active' or 'suspended'.");
    }

    private static bool IsSupportedStatus(string? status)
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