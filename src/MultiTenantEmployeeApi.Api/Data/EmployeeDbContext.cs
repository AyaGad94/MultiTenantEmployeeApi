using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Entities;

namespace MultiTenantEmployeeApi.Api.Data;

public sealed class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EmployeeDbContext).Assembly);
    }
}