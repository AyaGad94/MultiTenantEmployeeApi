namespace MultiTenantEmployeeApi.Api.Common.Tenancy;

public sealed class TenantContext : ITenantContext
{
    private Guid? _tenantId;

    public Guid TenantId =>
        _tenantId
        ?? throw new InvalidOperationException(
            "The tenant has not been resolved for the current request.");

    public void SetTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));
        }

        if (_tenantId.HasValue)
        {
            throw new InvalidOperationException(
                "The tenant has already been resolved for the current request.");
        }

        _tenantId = tenantId;
    }
}