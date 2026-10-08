Multi-Tenant Employee API
A small ASP.NET Core API for managing employees across multiple tenants.
The project focuses on tenant isolation, clear API behavior, simple CQRS, real PostgreSQL integration testing, and a few PostgreSQL/EF Core features that make the implementation safer and easier to reason about.
Tech stack
- .NET 9 / ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 16
- MediatR
- FluentValidation
- xUnit
- Testcontainers
- Docker / Docker Compose
Main requirements
The API supports the following employee operations:
Method	Endpoint	Purpose
POST	/api/v1/employees	Create an employee
GET	/api/v1/employees	List employees with pagination and filtering
GET	/api/v1/employees/{id}	Get one employee
PUT	/api/v1/employees/{id}	Update an employee
DELETE	/api/v1/employees/{id}	Soft-delete an employee


Every request is scoped to a tenant through the X-Tenant-Id header.
Configured tenant IDs:
Tenant A: 11111111-1111-1111-1111-111111111111
Tenant B: 22222222-2222-2222-2222-222222222222
Tenant A cannot access Tenant B employees, and Tenant B cannot access Tenant A employees.
Employee model
The employees table contains:
id
tenant_id
first_name
last_name
email
department
status
custom_data
amount_minor
currency_code
created_at
updated_at
deleted_at
status is limited to:
active
suspended
Delete is implemented as a soft delete by setting deleted_at.
CQRS and MediatR
The project uses a simple CQRS approach with MediatR.
Writes are handled as commands and reads are handled as queries.
The employee feature is split by operation:
Create
Delete
GetById
List
Update
This keeps each handler small and makes the behavior easier to test.
Validation
FluentValidation is used before executing Create and Update operations.
Validation covers:
- required employee fields
- email format
- employee status
- pagination values
- tenant-specific custom_data
- salary values when salary is supplied
Response format
API responses use a common envelope:
{
  "data": {},
  "pagination": null,
  "error": null
}
List responses use the pagination field, while validation and business errors use the error field.
Error handling
The API keeps error responses consistent by using the same response envelope for application errors and ASP.NET Core model-binding errors.
Handled cases include:
- FluentValidation validation errors
- duplicate employee email
- employee not found
- missing or invalid tenant header
- unsupported tenant
- malformed JSON
- model-binding errors
ASP.NET Core automatic 400 Bad Request responses are configured through ApiBehaviorOptions.InvalidModelStateResponseFactory so they return the project response envelope instead of the default ProblemDetails format.
For example, malformed JSON returns:
{
  "data": null,
  "pagination": null,
  "error": "The request body is invalid."
}
An integration test also verifies that malformed JSON returns HTTP 400, includes data, pagination, and error, and does not return the default ProblemDetails fields such as type and title.
Multi-tenancy
The current tenant is resolved from:
X-Tenant-Id
The tenant context is scoped per request.
Tenant isolation is enforced in more than one place:
1. EF Core global query filters
2. PostgreSQL Row-Level Security
That means tenant protection does not depend on every developer remembering to manually add tenant_id conditions to every query.
Pagination and filtering
GET /api/v1/employees supports pagination and filtering.
Example:
GET /api/v1/employees?page=1&pageSize=20&department=Engineering&status=active
The read handler uses:
- AsNoTracking()
- filtering before pagination
- deterministic ordering
- Select() projections
- async EF Core queries with CancellationToken
custom_data
custom_data is stored as PostgreSQL JSONB.
It is optional.
Each tenant has its own allowed custom fields.
Tenant A:
Field	Type
jobLevel	string
officeLocation	string
yearsExperience	integer


Tenant B:
Field	Type
employeeCode	string
remote	boolean
costCenter	string


Unknown custom fields and values with the wrong JSON type are rejected during Create and Update.
Bonus requirements
All five bonus requirements are implemented.
Bonus 1 — EF Core Global Query Filters
The project uses an EF Core global query filter for Employee.
The filter automatically applies:
tenant_id = current tenant
deleted_at IS NULL
This means normal employee queries automatically:
- stay inside the current tenant
- hide soft-deleted employees
Handlers do not need to repeat the same tenant and soft-delete conditions in every query.
Bonus 2 — Tenant-specific custom_data validation
Each tenant has its own custom field definitions.
The validator checks:
- whether the field is allowed for the current tenant
- whether the JSON value has the expected type
For example, Tenant A can use:
{
  "jobLevel": "Senior",
  "officeLocation": "Alexandria",
  "yearsExperience": 5
}
Tenant B can use:
{
  "employeeCode": "EMP-100",
  "remote": true,
  "costCenter": "CC-01"
}
A Tenant B-only field such as remote is rejected when it is sent for Tenant A.
Bonus 3 — PostgreSQL Row-Level Security
PostgreSQL RLS is enabled and forced on the employees table.
The current tenant ID is written into the PostgreSQL session as:
app.current_tenant_id
The database policy checks that value for reads and writes.
The policy protects:
SELECT
INSERT
UPDATE
DELETE
The project also includes real PostgreSQL integration tests for RLS.
One test intentionally calls:
IgnoreQueryFilters()
to bypass the EF Core tenant filter.
PostgreSQL still returns only the current tenant rows, which proves that tenant isolation is also enforced at the database level.
Another test confirms that a Tenant A connection cannot insert an employee whose tenant_id belongs to Tenant B.
Bonus 4 — Audit logging
The project contains a separate:
audit_logs
table.
It records:
id
tenant_id
employee_id
action
occurred_at
Create operations generate:
Created
audit records.
Update operations generate:
Updated
audit records.
There is no authentication/user model in the task, so tenant_id is the actor context available for identifying who performed the operation.
The employee change and its audit log are written in the same SaveChangesAsync() call.
Tests also verify that rejected operations, such as a duplicate email Create request, do not create false audit rows.
Bonus 5 — Money value object for salary
Salary is implemented using a Money value object.
The application model is:
Money
├── AmountMinor
└── CurrencyCode
The PostgreSQL representation is:
amount_minor   INTEGER
currency_code  TEXT
No floating-point or decimal salary column is used.
Example:
{
  "salary": {
    "amountMinor": 300000,
    "currencyCode": "USD"
  }
}
Currency codes are normalized to uppercase by the value object.
Salary is optional, so both salary columns can be null when salary is not supplied.
The salary value is supported by:
- Create
- Update
- Get by ID
- List
The implementation was also verified directly in PostgreSQL:
amount_minor = 300000
currency_code = USD
Audit logging table
The audit table is separate from employees.
Example structure:
audit_logs
├── id
├── tenant_id
├── employee_id
├── action
└── occurred_at
Keeping audit data separate avoids adding audit-specific columns to the employee model.
Salary example
Create an employee with salary:
{
  "firstName": "Aya",
  "lastName": "Gad",
  "email": "aya@example.com",
  "department": "Engineering",
  "status": "active",
  "customData": {
    "jobLevel": "Senior",
    "officeLocation": "Alexandria",
    "yearsExperience": 5
  },
  "salary": {
    "amountMinor": 250000,
    "currencyCode": "EGP"
  }
}
A later PUT request can replace that salary:
{
  "firstName": "Aya",
  "lastName": "Gad",
  "email": "aya@example.com",
  "department": "Platform Engineering",
  "status": "active",
  "customData": {
    "jobLevel": "Lead",
    "officeLocation": "Alexandria",
    "yearsExperience": 6
  },
  "salary": {
    "amountMinor": 300000,
    "currencyCode": "USD"
  }
}
Running with Docker
Create a local environment file from the example:
Copy-Item .env.example .env
Start PostgreSQL and the API:
docker compose up --build -d
The API runs at:
http://localhost:8080
Check the containers:
docker compose ps
Stop them:
docker compose down
PostgreSQL data remains in the Docker volume unless the volume is explicitly removed.
Database migrations
EF Core migrations are located under:
src/MultiTenantEmployeeApi.Api/Data/Migrations
The migration history covers:
InitialCreate
AddEmployeeRowLevelSecurity
AddAuditLogging
AddEmployeeSalary
The API applies pending migrations on startup.
Useful commands:
dotnet ef migrations list `
  --project .\src\MultiTenantEmployeeApi.Api
dotnet ef database update `
  --project .\src\MultiTenantEmployeeApi.Api
To verify that the EF model and migration snapshot are synchronized:
dotnet ef migrations has-pending-model-changes `
  --project .\src\MultiTenantEmployeeApi.Api
Tests
Run all tests:
dotnet test
The test suite covers:
- Create employee happy path
- duplicate email handling
- list pagination
- tenant isolation
- soft-delete behavior
- tenant-specific custom_data
- Create audit logging
- Update audit logging
- Money value object behavior
- PostgreSQL integration
- PostgreSQL Row-Level Security
- cross-tenant RLS write protection
Integration tests use a real PostgreSQL database through Testcontainers.
EF Core InMemory is used only for focused unit tests.
Project structure
src/
  MultiTenantEmployeeApi.Api/
    Common/
      CustomData/
      Money/
      Responses/
      Tenancy/
    Controllers/
    Data/
      Configurations/
      Interceptors/
      Migrations/
    Entities/
      Audit/
    Features/
      Employees/
        Create/
        Delete/
        GetById/
        List/
        Update/
    ValueObjects/

tests/
  MultiTenantEmployeeApi.UnitTests/
  MultiTenantEmployeeApi.IntegrationTests/
Design decisions
A few decisions were intentionally kept simple.
The two tenants use fixed configured IDs because the task requires two tenants but does not require a tenant-management feature or tenant table.
Authentication is not included because it is not part of the task. For the same reason, audit logging uses the current tenant as the available actor context instead of introducing a user or JWT model that the task does not ask for.
Tenant isolation is implemented twice on purpose. EF Core global query filters make normal application queries safe by default, while PostgreSQL RLS protects the data at the database layer.
Salary is stored as integer minor units plus a currency code so the project never stores money as a floating-point value.
The overall structure is intentionally small enough to explain easily in a technical interview without hiding the important behavior behind unnecessary abstraction.