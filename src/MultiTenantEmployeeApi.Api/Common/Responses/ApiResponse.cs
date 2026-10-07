namespace MultiTenantEmployeeApi.Api.Common.Responses;

public sealed class ApiResponse<T>
{
    public T? Data { get; init; }

    public object? Pagination { get; init; }

    public string? Error { get; init; }

    public static ApiResponse<T> Success(
        T data,
        object? pagination = null)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Pagination = pagination,
            Error = null
        };
    }

    public static ApiResponse<T> Failure(string errorMessage)
    {
        return new ApiResponse<T>
        {
            Data = default,
            Pagination = null,
            Error = errorMessage
        };
    }
}