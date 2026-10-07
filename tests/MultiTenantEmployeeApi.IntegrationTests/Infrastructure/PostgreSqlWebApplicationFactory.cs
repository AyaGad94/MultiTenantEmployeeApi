using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantEmployeeApi.Api.Data;

namespace MultiTenantEmployeeApi.IntegrationTests.Infrastructure;

public sealed class PostgreSqlWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;

    public PostgreSqlWebApplicationFactory(
        string postgresConnectionString)
    {
        _postgresConnectionString = postgresConnectionString;
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextConfiguration = services
                .SingleOrDefault(serviceDescriptor =>
                    serviceDescriptor.ServiceType ==
                    typeof(
                        IDbContextOptionsConfiguration<EmployeeDbContext>));

            if (dbContextConfiguration is not null)
            {
                services.Remove(dbContextConfiguration);
            }

            services.AddDbContext<EmployeeDbContext>(options =>
                options.UseNpgsql(_postgresConnectionString));
        });
    }
}