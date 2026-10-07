namespace MultiTenantEmployeeApi.Api.Common.Responses;

public sealed class PaginationMetadata
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }
}