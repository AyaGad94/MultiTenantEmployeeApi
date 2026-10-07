using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MultiTenantEmployeeApi.Api.Common.Responses;
using MultiTenantEmployeeApi.Api.Features.Employees.Create;
using MultiTenantEmployeeApi.Api.Features.Employees.List;

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

    public EmployeesController(
        ISender sender,
        IValidator<CreateEmployeeCommand> createEmployeeValidator,
        IValidator<ListEmployeesQuery> listEmployeesValidator)
    {
        _sender = sender;
        _createEmployeeValidator = createEmployeeValidator;
        _listEmployeesValidator = listEmployeesValidator;
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
}