using MediatR;

namespace MultiTenantEmployeeApi.Api.Features.Employees.GetById;

public sealed record GetEmployeeByIdQuery(Guid EmployeeId)
    : IRequest<EmployeeDetailsResponse?>;