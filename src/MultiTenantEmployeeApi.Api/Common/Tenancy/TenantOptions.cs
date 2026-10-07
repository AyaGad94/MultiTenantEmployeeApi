namespace MultiTenantEmployeeApi.Api.Common.Tenancy;

public sealed class TenantOptions
{
    public const string SectionName = "Tenants";

    public Guid TenantAId { get; init; }

    public Guid TenantBId { get; init; }
}