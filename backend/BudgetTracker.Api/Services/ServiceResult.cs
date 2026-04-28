using BudgetTracker.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Services;

public class ServiceResult<T>
{
    public T? Value { get; init; }
    public ApiErrorDto? Error { get; init; }
    public int StatusCode { get; init; } = StatusCodes.Status200OK;
    public bool IsSuccess => Error is null;

    public static ServiceResult<T> Success(T value, int statusCode = StatusCodes.Status200OK)
    {
        return new ServiceResult<T> { Value = value, StatusCode = statusCode };
    }

    public static ServiceResult<T> Failure(string code, string message, int statusCode)
    {
        return new ServiceResult<T>
        {
            Error = ApiErrorDto.Create(code, message),
            StatusCode = statusCode
        };
    }
}

public class ServiceResult
{
    public ApiErrorDto? Error { get; init; }
    public int StatusCode { get; init; } = StatusCodes.Status204NoContent;
    public bool IsSuccess => Error is null;

    public static ServiceResult Success(int statusCode = StatusCodes.Status204NoContent)
    {
        return new ServiceResult { StatusCode = statusCode };
    }

    public static ServiceResult Failure(string code, string message, int statusCode)
    {
        return new ServiceResult
        {
            Error = ApiErrorDto.Create(code, message),
            StatusCode = statusCode
        };
    }
}

public static class ServiceResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode == StatusCodes.Status201Created
                ? controller.StatusCode(result.StatusCode, result.Value)
                : controller.Ok(result.Value);
        }

        return controller.StatusCode(result.StatusCode, result.Error);
    }

    public static IActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        if (result.IsSuccess)
        {
            return controller.StatusCode(result.StatusCode);
        }

        return controller.StatusCode(result.StatusCode, result.Error);
    }
}
