namespace MultiTenantEmployeeApi.Api.Common.CustomData;

public sealed record CustomFieldDefinition(
    string Name,
    CustomFieldType Type);