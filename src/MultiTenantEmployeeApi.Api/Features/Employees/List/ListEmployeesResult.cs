using MultiTenantEmployeeApi.Api.Common.Responses;

namespace MultiTenantEmployeeApi.Api.Features.Employees.List;

public sealed class ListEmployeesResult
{
    public IReadOnlyList<ListEmployeeItem> Employees { get; init; }
        = Array.Empty<ListEmployeeItem>();

    public PaginationMetadata Pagination { get; init; }
        = new();
}