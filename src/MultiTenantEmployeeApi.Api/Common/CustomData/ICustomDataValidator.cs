using System.Text.Json;

namespace MultiTenantEmployeeApi.Api.Common.CustomData;

public interface ICustomDataValidator
{
    IReadOnlyList<string> Validate(
        Guid tenantId,
        JsonElement? customData);
}