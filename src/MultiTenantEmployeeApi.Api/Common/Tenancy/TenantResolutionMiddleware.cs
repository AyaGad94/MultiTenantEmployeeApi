using Microsoft.Extensions.Options;

namespace MultiTenantEmployeeApi.Api.Common.Tenancy;

public sealed class TenantResolutionMiddleware
{
    public const string TenantHeaderName = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        TenantContext tenantContext,
        IOptions<TenantOptions> tenantOptions)
    {
        if (!httpContext.Request.Headers.TryGetValue(
                TenantHeaderName,
                out var tenantHeaderValues))
        {
            await WriteTenantErrorAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                $"The {TenantHeaderName} header is required.");

            return;
        }

        if (tenantHeaderValues.Count != 1)
        {
            await WriteTenantErrorAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                $"The {TenantHeaderName} header must contain one value.");

            return;
        }

        var tenantHeaderValue = tenantHeaderValues[0];

        if (!Guid.TryParse(tenantHeaderValue, out var currentTenantId))
        {
            await WriteTenantErrorAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                $"The {TenantHeaderName} header must contain a valid UUID.");

            return;
        }

        var configuredTenants = tenantOptions.Value;

        var tenantIsSupported =
            currentTenantId == configuredTenants.TenantAId ||
            currentTenantId == configuredTenants.TenantBId;

        if (!tenantIsSupported)
        {
            await WriteTenantErrorAsync(
                httpContext,
                StatusCodes.Status403Forbidden,
                "The requested tenant is not supported.");

            return;
        }

        tenantContext.SetTenantId(currentTenantId);

        await _next(httpContext);
    }

    private static async Task WriteTenantErrorAsync(
        HttpContext httpContext,
        int statusCode,
        string errorMessage)
    {
        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsync(
            errorMessage,
            httpContext.RequestAborted);
    }
}