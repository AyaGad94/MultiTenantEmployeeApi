using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Entities;
using MultiTenantEmployeeApi.Api.Entities.Audit;

namespace MultiTenantEmployeeApi.Api.Data;

public sealed class EmployeeDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public EmployeeDbContext(
        DbContextOptions<EmployeeDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    private Guid CurrentTenantId => _tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EmployeeDbContext).Assembly);

        modelBuilder.Entity<Employee>()
            .HasQueryFilter(employee =>
                employee.TenantId == CurrentTenantId &&
                employee.DeletedAt == null);
    }
}