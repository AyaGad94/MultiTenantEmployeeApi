using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiTenantEmployeeApi.Api.Entities;

namespace MultiTenantEmployeeApi.Api.Data.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable(
            "employees",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "ck_employees_status",
                    "status IN ('active', 'suspended')");
            });

        builder.HasKey(employee => employee.Id)
            .HasName("pk_employees");

        builder.Property(employee => employee.Id)
            .HasColumnName("id");

        builder.Property(employee => employee.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(employee => employee.FirstName)
            .HasColumnName("first_name")
            .IsRequired();

        builder.Property(employee => employee.LastName)
            .HasColumnName("last_name")
            .IsRequired();

        builder.Property(employee => employee.Email)
            .HasColumnName("email")
            .IsRequired();

        builder.Property(employee => employee.Department)
            .HasColumnName("department")
            .IsRequired();

        builder.Property(employee => employee.Status)
            .HasColumnName("status")
            .HasConversion(
                status => status.ToString().ToLowerInvariant(),
                storedStatus => Enum.Parse<EmployeeStatus>(
                    storedStatus,
                    true))
            .IsRequired();

        builder.Property(employee => employee.CustomData)
            .HasColumnName("custom_data")
            .HasColumnType("jsonb")
            .HasConversion(
                customData => customData == null
                    ? null
                    : customData.RootElement.GetRawText(),
                storedJson => storedJson == null
                    ? null
                    : JsonDocument.Parse(
                        storedJson,
                        default(JsonDocumentOptions)));

        builder.OwnsOne(
            employee => employee.Salary,
            salaryBuilder =>
            {
                salaryBuilder.Property(salary => salary.AmountMinor)
                    .HasColumnName("amount_minor")
                    .HasColumnType("integer");

                salaryBuilder.Property(salary => salary.CurrencyCode)
                    .HasColumnName("currency_code");
            });

        builder.Property(employee => employee.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(employee => employee.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(employee => employee.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp with time zone");
    }
}