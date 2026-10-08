namespace MultiTenantEmployeeApi.Api.Common.Tenancy;

public interface ITenantContext
{
    Guid TenantId { get; }

    bool TryGetTenantId(out Guid tenantId);
}