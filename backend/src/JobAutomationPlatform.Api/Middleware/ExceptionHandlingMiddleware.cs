using System.Net;
using JobAutomationPlatform.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace JobAutomationPlatform.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict detected.");
            await WriteProblemAsync(context, HttpStatusCode.Conflict, "The resource was modified by another request.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = (int)statusCode,
            Title = statusCode.ToString(),
            Detail = detail,
            Instance = context.Request.Path
        });
    }
}
