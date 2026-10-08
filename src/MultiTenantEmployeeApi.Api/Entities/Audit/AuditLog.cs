namespace MultiTenantEmployeeApi.Api.Entities.Audit;

public sealed class AuditLog
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid EmployeeId { get; set; }

    public AuditAction Action { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}