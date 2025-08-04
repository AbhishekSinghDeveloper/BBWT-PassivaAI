using BBWM.Core.Exceptions;
using BBWM.Core.Extensions;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Text.Json;

namespace BBWM.Core.Web.Middlewares;

public class ErrorHandlingMiddleware
{
    private const string FailedRequestItemKey = "ErrorHandlingMiddleware_FailedRequest";
    private const string StatusCodeItemKey = "ErrorHandlingMiddleware_StatusCode";
    private const string ContentTypeItemKey = "ErrorHandlingMiddleware_ContentType";

    private readonly RequestDelegate next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IErrorNotifyService _errorNotifyService;

    public static readonly BlockingCollection<Exception> HandlerExceptionsDebug = new BlockingCollection<Exception>();

    public ErrorHandlingMiddleware(RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment hostingEnvironment,
        IErrorNotifyService reportProblemService)
    {
        this.next = next;
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
        _errorNotifyService = reportProblemService;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            context.Response.OnStarting(() =>
            {
                if (context.Items.TryGetValue(FailedRequestItemKey, out var isFailedRequest) && (bool)isFailedRequest)
                {
                    context.Response.ContentType = (string)context.Items[ContentTypeItemKey];
                    context.Response.StatusCode = (int)context.Items[StatusCodeItemKey];
                }
                return Task.CompletedTask;
            });
            await next(context);
        }
        catch (Exception ex)
        {
            HandlerExceptionsDebug.Add(ex);
            if (HandlerExceptionsDebug.Count > 10) HandlerExceptionsDebug.Take();

            await HandleException(context, ex);
        }
    }

    private async Task HandleException(HttpContext context, Exception ex)
    {
        // Logging of error ID into the application logs in combination with the error message containing the same ID
        // and shown for end-user, has the main purpose - to track the error in logs after the end-user's feedback
        // (screenshot/movie).
        // Also, it's not necessary this error ID to be that unique. Therefore it's 6 lenght random string.
        var errorId = StringExtensions.RandomAlphaNumberic(6);
        _logger.LogError(ex, $"Error Handling Middlware (Error ID: {errorId})");

        int code;
        string message = ex.Message;

        switch (ex)
        {
            case ActionNotImplementedException _: code = StatusCodes.Status501NotImplemented; break;
            case ForbiddenException _: code = StatusCodes.Status403Forbidden; break;
            case EntityNotFoundException _: code = StatusCodes.Status404NotFound; break;
            case ConflictException _: code = StatusCodes.Status409Conflict; break;
            case ApiException _: code = StatusCodes.Status400BadRequest; break;
            case BusinessException _: code = StatusCodes.Status400BadRequest; break;
            case ValidationException validationException:
                code = StatusCodes.Status400BadRequest;
                message = FormatValidationException(validationException);
                break;

            default:
                if (!_hostingEnvironment.IsDevelopment())
                {
                    try
                    {
                        await _errorNotifyService.NotifyOnException(ex);
                    }
                    catch
                    {
                        _logger.LogError(ex, "An error occurred while sending an exception report.");
                    }
                }

                code = StatusCodes.Status500InternalServerError;

                message = $"[Error ID: {errorId}] {ex.Message}";
                if (!string.IsNullOrEmpty(ex.InnerException?.Message))
                {
                    message += $" Error details: {ex.InnerException?.Message}";
                }
                break;
        }

        context.Items.Add(FailedRequestItemKey, true);
        context.Items.Add(StatusCodeItemKey, code);
        context.Items.Add(ContentTypeItemKey, MediaTypeNames.Application.Json);

        await context.Response.WriteAsync(message);
    }

    private static string FormatValidationException(ValidationException ex)
    {
        ValidationProblemDetails GetValidationResult(ValidationException e)
        {
            if (e.ValidationResult.MemberNames is null || !e.ValidationResult.MemberNames.Any())
            {
                return new ValidationProblemDetails(
                    new Dictionary<string, string[]> { { "DTO", new[] { e.ValidationResult.ErrorMessage } } });
            }
            return new ValidationProblemDetails(
                e.ValidationResult.MemberNames.ToDictionary(key => key, value => new[] { e.ValidationResult.ErrorMessage }));
        }
        return JsonSerializer.Serialize(GetValidationResult(ex));
    }
}
