// using Microsoft.EntityFrameworkCore;
// using MultiTenantEmployeeApi.Api.Entities;

// namespace MultiTenantEmployeeApi.Api.Data;

// public sealed class EmployeeDbContext : DbContext
// {
//     public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
//         : base(options)
//     {
//     }

//     public DbSet<Employee> Employees => Set<Employee>();

//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder.ApplyConfigurationsFromAssembly(
//             typeof(EmployeeDbContext).Assembly);
//     }
// }
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using MultiTenantEmployeeApi.Api.Entities;

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