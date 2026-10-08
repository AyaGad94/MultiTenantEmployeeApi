using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MultiTenantEmployeeApi.Api.Common.Responses;
using MultiTenantEmployeeApi.Api.Features.Employees.Create;
using MultiTenantEmployeeApi.Api.Features.Employees.List;
using MultiTenantEmployeeApi.Api.Features.Employees.GetById;
using MultiTenantEmployeeApi.Api.Features.Employees.Update;
using MultiTenantEmployeeApi.Api.Features.Employees.Delete;

namespace MultiTenantEmployeeApi.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly ISender _sender;

    private readonly IValidator<CreateEmployeeCommand>
        _createEmployeeValidator;

    private readonly IValidator<ListEmployeesQuery>
        _listEmployeesValidator;

    private readonly IValidator<UpdateEmployeeCommand>
        _updateEmployeeValidator;

    public EmployeesController(
        ISender sender,
        IValidator<CreateEmployeeCommand> createEmployeeValidator,
        IValidator<ListEmployeesQuery> listEmployeesValidator,
        IValidator<UpdateEmployeeCommand> updateEmployeeValidator)
    {
        _sender = sender;
        _createEmployeeValidator = createEmployeeValidator;
        _listEmployeesValidator = listEmployeesValidator;
        _updateEmployeeValidator = updateEmployeeValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await _createEmployeeValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrorMessage = string.Join(
                "; ",
                validationResult.Errors.Select(
                    validationFailure =>
                        validationFailure.ErrorMessage));

            return BadRequest(
                ApiResponse<object?>.Failure(
                    validationErrorMessage));
        }

        var createResult = await _sender.Send(
            command,
            cancellationToken);

        if (createResult.EmailAlreadyExists)
        {
            return Conflict(
                ApiResponse<object?>.Failure(
                    "An employee with this email already exists for the current tenant."));
        }

        var response = new CreateEmployeeResponse
        {
            Id = createResult.EmployeeId!.Value
        };

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CreateEmployeeResponse>.Success(response));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListEmployeesQuery
        {
            Page = page,
            PageSize = pageSize,
            Department = department,
            Status = status
        };

        var validationResult =
            await _listEmployeesValidator.ValidateAsync(
                query,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrorMessage = string.Join(
                "; ",
                validationResult.Errors.Select(
                    validationFailure =>
                        validationFailure.ErrorMessage));

            return BadRequest(
                ApiResponse<object?>.Failure(
                    validationErrorMessage));
        }

        var listResult = await _sender.Send(
            query,
            cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyList<ListEmployeeItem>>.Success(
                listResult.Employees,
                listResult.Pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetEmployeeByIdQuery(id);

        var employee = await _sender.Send(
            query,
            cancellationToken);

        if (employee is null)
        {
            return NotFound(
                ApiResponse<object?>.Failure(
                    "Employee was not found."));
        }

        return Ok(
            ApiResponse<EmployeeDetailsResponse>.Success(
                employee));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand
        {
            EmployeeId = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Department = request.Department,
            Status = request.Status,
            CustomData = request.CustomData,
            Salary = request.Salary
        };

        var validationResult =
            await _updateEmployeeValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            var validationErrorMessage = string.Join(
                "; ",
                validationResult.Errors.Select(
                    validationFailure =>
                        validationFailure.ErrorMessage));

            return BadRequest(
                ApiResponse<object?>.Failure(
                    validationErrorMessage));
        }

        var updateResult = await _sender.Send(
            command,
            cancellationToken);

        if (updateResult.EmployeeNotFound)
        {
            return NotFound(
                ApiResponse<object?>.Failure(
                    "Employee was not found."));
        }

        if (updateResult.EmailAlreadyExists)
        {
            return Conflict(
                ApiResponse<object?>.Failure(
                    "An employee with this email already exists for the current tenant."));
        }

        var response = new UpdateEmployeeResponse
        {
            Id = updateResult.EmployeeId!.Value
        };

        return Ok(
            ApiResponse<UpdateEmployeeResponse>.Success(
                response));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(
                ApiResponse<object?>.Failure(
                    "Employee ID must be a valid non-empty UUID."));
        }

        var command = new DeleteEmployeeCommand(id);

        var deletedEmployeeId = await _sender.Send(
            command,
            cancellationToken);

        if (!deletedEmployeeId.HasValue)
        {
            return NotFound(
                ApiResponse<object?>.Failure(
                    "Employee was not found."));
        }

        var response = new DeleteEmployeeResponse
        {
            Id = deletedEmployeeId.Value
        };

        return Ok(
            ApiResponse<DeleteEmployeeResponse>.Success(
                response));
    }
}