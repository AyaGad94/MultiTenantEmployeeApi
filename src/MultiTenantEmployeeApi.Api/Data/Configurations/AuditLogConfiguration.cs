using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiTenantEmployeeApi.Api.Entities.Audit;

namespace MultiTenantEmployeeApi.Api.Data.Configurations;

public sealed class AuditLogConfiguration
    : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(
        EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(auditLog => auditLog.Id)
            .HasName("pk_audit_logs");

        builder.Property(auditLog => auditLog.Id)
            .HasColumnName("id");

        builder.Property(auditLog => auditLog.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(auditLog => auditLog.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(auditLog => auditLog.Action)
            .HasColumnName("action")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(auditLog => auditLog.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}