using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var postgresConnectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' is not configured.");

builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();