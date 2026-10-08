using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MultiTenantEmployeeApi.Api.Common.Tenancy;
using Npgsql;

namespace MultiTenantEmployeeApi.Api.Data.Interceptors;

public sealed class TenantSessionConnectionInterceptor
    : DbConnectionInterceptor
{
    private const string TenantSettingName =
        "app.current_tenant_id";

    private readonly ITenantContext _tenantContext;

    public TenantSessionConnectionInterceptor(
        ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (connection is not NpgsqlConnection postgresConnection)
        {
            return;
        }

        await using var command =
            postgresConnection.CreateCommand();

        if (_tenantContext.TryGetTenantId(
                out var currentTenantId))
        {
            command.CommandText =
                """
                SELECT set_config(
                    'app.current_tenant_id',
                    @tenantId,
                    false);
                """;

            command.Parameters.AddWithValue(
                "tenantId",
                currentTenantId.ToString());
        }
        else
        {
            command.CommandText =
                """
                SELECT set_config(
                    'app.current_tenant_id',
                    '',
                    false);
                """;
        }

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }
}