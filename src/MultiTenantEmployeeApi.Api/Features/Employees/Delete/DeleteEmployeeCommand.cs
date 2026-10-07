using MediatR;

namespace MultiTenantEmployeeApi.Api.Features.Employees.Delete;

public sealed record DeleteEmployeeCommand(Guid EmployeeId)
    : IRequest<Guid?>;