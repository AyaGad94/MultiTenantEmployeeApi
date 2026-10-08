using System.Text.Json;
using Microsoft.Extensions.Options;
using MultiTenantEmployeeApi.Api.Common.CustomData;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Features.Employees.Create;
using MultiTenantEmployeeApi.Api.Features.Employees.Update;

namespace MultiTenantEmployeeApi.UnitTests.Features.Employees.Validation;

public sealed class CustomDataValidationTests
{
    private static readonly Guid TenantAId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid TenantBId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void CreateValidator_AcceptsValidTenantACustomData()
    {
        var tenantContext =
            CreateTenantContext(TenantAId);

        var validator =
            CreateCreateValidator(tenantContext);

        var command = CreateValidCreateCommand(
            ParseJson(
                """
                {
                  "jobLevel": "Senior",
                  "officeLocation": "Alexandria",
                  "yearsExperience": 5
                }
                """));

        var validationResult =
            validator.Validate(command);

        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public void CreateValidator_RejectsWrongTypeForTenantAField()
    {
        var tenantContext =
            CreateTenantContext(TenantAId);

        var validator =
            CreateCreateValidator(tenantContext);

        var command = CreateValidCreateCommand(
            ParseJson(
                """
                {
                  "jobLevel": 5
                }
                """));

        var validationResult =
            validator.Validate(command);

        Assert.False(validationResult.IsValid);

        Assert.Contains(
            validationResult.Errors,
            error =>
                error.ErrorMessage.Contains(
                    "jobLevel",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void CreateValidator_RejectsTenantBFieldForTenantA()
    {
        var tenantContext =
            CreateTenantContext(TenantAId);

        var validator =
            CreateCreateValidator(tenantContext);

        var command = CreateValidCreateCommand(
            ParseJson(
                """
                {
                  "employeeCode": "EMP-100"
                }
                """));

        var validationResult =
            validator.Validate(command);

        Assert.False(validationResult.IsValid);

        Assert.Contains(
            validationResult.Errors,
            error =>
                error.ErrorMessage.Contains(
                    "employeeCode",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void CreateValidator_AcceptsValidTenantBCustomData()
    {
        var tenantContext =
            CreateTenantContext(TenantBId);

        var validator =
            CreateCreateValidator(tenantContext);

        var command = CreateValidCreateCommand(
            ParseJson(
                """
                {
                  "employeeCode": "EMP-100",
                  "remote": true,
                  "costCenter": "CC-20"
                }
                """));

        var validationResult =
            validator.Validate(command);

        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public void UpdateValidator_AcceptsValidTenantACustomData()
    {
        var tenantContext =
            CreateTenantContext(TenantAId);

        var validator =
            CreateUpdateValidator(tenantContext);

        var command = CreateValidUpdateCommand(
            ParseJson(
                """
                {
                  "jobLevel": "Lead",
                  "officeLocation": "Alexandria",
                  "yearsExperience": 7
                }
                """));

        var validationResult =
            validator.Validate(command);

        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public void UpdateValidator_RejectsInvalidTenantACustomData()
    {
        var tenantContext =
            CreateTenantContext(TenantAId);

        var validator =
            CreateUpdateValidator(tenantContext);

        var command = CreateValidUpdateCommand(
            ParseJson(
                """
                {
                  "remote": true
                }
                """));

        var validationResult =
            validator.Validate(command);

        Assert.False(validationResult.IsValid);

        Assert.Contains(
            validationResult.Errors,
            error =>
                error.ErrorMessage.Contains(
                    "remote",
                    StringComparison.Ordinal));
    }

    private static CreateEmployeeValidator CreateCreateValidator(
        ITenantContext tenantContext)
    {
        var customDataValidator =
            CreateCustomDataValidator();

        return new CreateEmployeeValidator(
            customDataValidator,
            tenantContext);
    }

    private static UpdateEmployeeValidator CreateUpdateValidator(
        ITenantContext tenantContext)
    {
        var customDataValidator =
            CreateCustomDataValidator();

        return new UpdateEmployeeValidator(
            customDataValidator,
            tenantContext);
    }

    private static CustomDataValidator CreateCustomDataValidator()
    {
        var tenantOptions =
            Options.Create(
                new TenantOptions
                {
                    TenantAId = TenantAId,
                    TenantBId = TenantBId
                });

        return new CustomDataValidator(
            tenantOptions);
    }

    private static CreateEmployeeCommand CreateValidCreateCommand(
        JsonElement customData)
    {
        return new CreateEmployeeCommand
        {
            FirstName = "Aya",
            LastName = "Gad",
            Email = "aya@example.com",
            Department = "Engineering",
            Status = "active",
            CustomData = customData
        };
    }

    private static UpdateEmployeeCommand CreateValidUpdateCommand(
        JsonElement customData)
    {
        return new UpdateEmployeeCommand
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = "Aya",
            LastName = "Gad",
            Email = "aya@example.com",
            Department = "Engineering",
            Status = "active",
            CustomData = customData
        };
    }

    private static TenantContext CreateTenantContext(
        Guid tenantId)
    {
        var tenantContext = new TenantContext();

        tenantContext.SetTenantId(tenantId);

        return tenantContext;
    }

    private static JsonElement ParseJson(
        string json)
    {
        using var jsonDocument =
            JsonDocument.Parse(json);

        return jsonDocument.RootElement.Clone();
    }
}