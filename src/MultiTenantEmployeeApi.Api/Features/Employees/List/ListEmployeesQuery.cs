using MediatR;

namespace MultiTenantEmployeeApi.Api.Features.Employees.List;

public sealed class ListEmployeesQuery : IRequest<ListEmployeesResult>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? Department { get; init; }

    public string? Status { get; init; }
}