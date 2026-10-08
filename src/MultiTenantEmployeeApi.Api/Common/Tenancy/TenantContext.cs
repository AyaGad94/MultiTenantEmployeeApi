namespace MultiTenantEmployeeApi.Api.Common.Tenancy;

public sealed class TenantContext : ITenantContext
{
    private Guid? _tenantId;

    public Guid TenantId =>
        _tenantId
        ?? throw new InvalidOperationException(
            "The tenant has not been resolved for the current request.");

    public bool TryGetTenantId(
        out Guid tenantId)
    {
        if (_tenantId.HasValue)
        {
            tenantId = _tenantId.Value;
            return true;
        }

        tenantId = Guid.Empty;
        return false;
    }

    public void SetTenantId(
        Guid tenantId)
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
                "The tenant has already been resolved for this request.");
        }

        _tenantId = tenantId;
    }
}