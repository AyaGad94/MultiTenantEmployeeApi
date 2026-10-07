using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Data;
using MultiTenantEmployeeApi.Api.Features.Employees.Create;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<EmployeeDbContext>(
    (serviceProvider, options) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var postgresConnectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is not configured.");

        options.UseNpgsql(postgresConnectionString);
    });

builder.Services
    .AddOptions<TenantOptions>()
    .Bind(
        builder.Configuration.GetSection(
            TenantOptions.SectionName))
    .Validate(
        tenantOptions => tenantOptions.TenantAId != Guid.Empty,
        "Tenant A ID must be configured.")
    .Validate(
        tenantOptions => tenantOptions.TenantBId != Guid.Empty,
        "Tenant B ID must be configured.")
    .Validate(
        tenantOptions =>
            tenantOptions.TenantAId != tenantOptions.TenantBId,
        "Tenant A and Tenant B must have different IDs.")
    .ValidateOnStart();

builder.Services.AddScoped<TenantContext>();

builder.Services.AddScoped<ITenantContext>(
    serviceProvider =>
        serviceProvider.GetRequiredService<TenantContext>());

builder.Services.AddMediatR(
    configuration =>
        configuration.RegisterServicesFromAssemblyContaining<
            CreateEmployeeCommand>());

builder.Services.AddValidatorsFromAssemblyContaining<
    CreateEmployeeValidator>();

var app = builder.Build();

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program
{
}