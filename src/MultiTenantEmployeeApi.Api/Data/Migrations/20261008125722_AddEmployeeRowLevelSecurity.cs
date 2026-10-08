using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantEmployeeApi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE employees
                ENABLE ROW LEVEL SECURITY;

                ALTER TABLE employees
                FORCE ROW LEVEL SECURITY;

                CREATE POLICY employees_tenant_isolation_policy
                ON employees
                USING (
                    tenant_id =
                    NULLIF(
                        current_setting(
                            'app.current_tenant_id',
                            true),
                        ''
                    )::uuid
                )
                WITH CHECK (
                    tenant_id =
                    NULLIF(
                        current_setting(
                            'app.current_tenant_id',
                            true),
                        ''
                    )::uuid
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP POLICY IF EXISTS
                employees_tenant_isolation_policy
                ON employees;

                ALTER TABLE employees
                NO FORCE ROW LEVEL SECURITY;

                ALTER TABLE employees
                DISABLE ROW LEVEL SECURITY;
                """);
        }
    }
}