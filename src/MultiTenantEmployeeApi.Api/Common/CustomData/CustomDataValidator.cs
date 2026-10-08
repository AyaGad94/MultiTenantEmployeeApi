using System.Text.Json;
using Microsoft.Extensions.Options;
using MultiTenantEmployeeApi.Api.Common.Tenancy;

namespace MultiTenantEmployeeApi.Api.Common.CustomData;

public sealed class CustomDataValidator
    : ICustomDataValidator
{
    private readonly IReadOnlyDictionary<
        Guid,
        IReadOnlyDictionary<string, CustomFieldDefinition>>
        _definitionsByTenant;

    public CustomDataValidator(
        IOptions<TenantOptions> tenantOptions)
    {
        var tenantAId =
            tenantOptions.Value.TenantAId;

        var tenantBId =
            tenantOptions.Value.TenantBId;

        _definitionsByTenant =
            new Dictionary<
                Guid,
                IReadOnlyDictionary<string, CustomFieldDefinition>>
            {
                [tenantAId] =
                    CreateDefinitions(
                        new CustomFieldDefinition(
                            "jobLevel",
                            CustomFieldType.String),
                        new CustomFieldDefinition(
                            "officeLocation",
                            CustomFieldType.String),
                        new CustomFieldDefinition(
                            "yearsExperience",
                            CustomFieldType.Integer)),

                [tenantBId] =
                    CreateDefinitions(
                        new CustomFieldDefinition(
                            "employeeCode",
                            CustomFieldType.String),
                        new CustomFieldDefinition(
                            "remote",
                            CustomFieldType.Boolean),
                        new CustomFieldDefinition(
                            "costCenter",
                            CustomFieldType.String))
            };
    }

    public IReadOnlyList<string> Validate(
        Guid tenantId,
        JsonElement? customData)
    {
        if (!customData.HasValue ||
            customData.Value.ValueKind ==
            JsonValueKind.Null)
        {
            return Array.Empty<string>();
        }

        var customDataValue = customData.Value;

        if (customDataValue.ValueKind !=
            JsonValueKind.Object)
        {
            return new[]
            {
                "CustomData must be a JSON object."
            };
        }

        if (!_definitionsByTenant.TryGetValue(
                tenantId,
                out var tenantDefinitions))
        {
            return new[]
            {
                "Custom data definitions are not configured for the current tenant."
            };
        }

        var validationErrors =
            new List<string>();

        foreach (var customField in
                 customDataValue.EnumerateObject())
        {
            if (!tenantDefinitions.TryGetValue(
                    customField.Name,
                    out var fieldDefinition))
            {
                validationErrors.Add(
                    $"CustomData field '{customField.Name}' is not allowed for the current tenant.");

                continue;
            }

            if (!HasExpectedType(
                    customField.Value,
                    fieldDefinition.Type))
            {
                validationErrors.Add(
                    $"CustomData field '{customField.Name}' must be {GetExpectedTypeName(fieldDefinition.Type)}.");
            }
        }

        return validationErrors;
    }

    private static IReadOnlyDictionary<
        string,
        CustomFieldDefinition> CreateDefinitions(
        params CustomFieldDefinition[] definitions)
    {
        return definitions.ToDictionary(
            definition => definition.Name,
            definition => definition,
            StringComparer.Ordinal);
    }

    private static bool HasExpectedType(
        JsonElement customFieldValue,
        CustomFieldType expectedType)
    {
        return expectedType switch
        {
            CustomFieldType.String =>
                customFieldValue.ValueKind ==
                JsonValueKind.String,

            CustomFieldType.Integer =>
                customFieldValue.ValueKind ==
                    JsonValueKind.Number &&
                customFieldValue.TryGetInt64(out _),

            CustomFieldType.Boolean =>
                customFieldValue.ValueKind is
                    JsonValueKind.True or
                    JsonValueKind.False,

            _ => false
        };
    }

    private static string GetExpectedTypeName(
        CustomFieldType customFieldType)
    {
        return customFieldType switch
        {
            CustomFieldType.String => "a string",
            CustomFieldType.Integer => "an integer",
            CustomFieldType.Boolean => "a boolean",
            _ => "a valid value"
        };
    }
}