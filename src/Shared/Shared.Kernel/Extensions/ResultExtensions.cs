using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Responses;
using Shared.Kernel.Results;

namespace Shared.Kernel.Extensions;


public static class ResultExtensions
{
  
    public static ActionResult<ResponseDTO> ToActionResult<T>(
        this Result<T> result,
        object? content = null)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(ResponseDTO.Success(result.Value, content))
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        return new ObjectResult(ResponseDTO.Error(result.Error!, content: content))
        {
            StatusCode = MapErrorCodeToHttpStatus(result.Error!.Code)
        };
    }


    public static ActionResult<ResponseDTO> ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        object? routeValues = null,
        object? content = null)
    {
        if (result.IsSuccess)
        {
            return new CreatedAtActionResult(
                actionName,
                null,
                routeValues,
                ResponseDTO.Success(result.Value, content))
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        return new ObjectResult(ResponseDTO.Error(result.Error!, content: content))
        {
            StatusCode = MapErrorCodeToHttpStatus(result.Error!.Code)
        };
    }


    public static ActionResult<ResponseDTO> ToActionResult(
        this Result result,
        object? content = null)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(ResponseDTO.Success(content: content))
            {
                StatusCode = StatusCodes.Status204NoContent
            };
        }

        return new ObjectResult(ResponseDTO.Error(result.Error!, content: content))
        {
            StatusCode = MapErrorCodeToHttpStatus(result.Error!.Code)
        };
    }


    private static int MapErrorCodeToHttpStatus(string errorCode)
    {
        return errorCode switch
        {
            // Success
            "0000" => StatusCodes.Status200OK,
            "0001" => StatusCodes.Status405MethodNotAllowed,  // Incorrect HTTP method
            "0002" => StatusCodes.Status400BadRequest,        // Invalid JSON
            "0003" => StatusCodes.Status401Unauthorized,      // Incorrect API Key
            "0004" => StatusCodes.Status401Unauthorized,      // API Key not provided
            "0005" => StatusCodes.Status400BadRequest,        // Invalid Reference
            "0006" => StatusCodes.Status409Conflict,          // Duplicated payment
            "0008" => StatusCodes.Status400BadRequest,        // Invalid Amount
            "0009" => StatusCodes.Status400BadRequest,        // Request ID not provided
            "0010" => StatusCodes.Status409Conflict,          // User already exists
            "0012" => StatusCodes.Status404NotFound,          // User not found
            "0013" => StatusCodes.Status401Unauthorized,      // Unauthorized
            "0014" => StatusCodes.Status404NotFound,          // Not Found
            "0015" => StatusCodes.Status400BadRequest,        // Validation error
            "0016" => StatusCodes.Status409Conflict,          // Already exists
            "1001" => StatusCodes.Status404NotFound,          // Parameter not found
            "1002" => StatusCodes.Status409Conflict,          // Code already exists
            "1003" => StatusCodes.Status400BadRequest,        // Invalid code
            "1004" => StatusCodes.Status400BadRequest,        // Cannot update inactive
            "0007" => StatusCodes.Status500InternalServerError,  // Internal error
            "0011" => StatusCodes.Status500InternalServerError,  // User creation failed
            "0017" => StatusCodes.Status500InternalServerError,  // Database error

            // Default
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

