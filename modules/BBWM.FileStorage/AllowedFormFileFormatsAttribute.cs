using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BBWM.FileStorage;

/// <summary>
/// Specifies allowed MIME types in files of the request's <see cref="Microsoft.AspNetCore.Http.IFormFileCollection"/> for an action.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AllowedFormFileFormatsAttribute : Attribute, IActionFilter
{
    private readonly string[] _allowedMimeTypes;
    private readonly long _maxFileSizeBytes = 0;


    /// <summary>
    /// Creates a new <see cref="AllowedFormFileFormatsAttribute" /> with given MIME types.
    /// </summary>
    /// <param name="mimeTypes">Allowed MIME types.</param>
    public AllowedFormFileFormatsAttribute(params string[] mimeTypes) =>
        _allowedMimeTypes = mimeTypes;

    /// <summary>
    /// Creates a new <see cref="AllowedFormFileFormatsAttribute" /> with given MIME types and file size.
    /// </summary>
    /// <param name="maxFileSizeBytes">Maximum file size in bytes.</param>
    /// <param name="mimeTypes">Allowed MIME types.</param>
    public AllowedFormFileFormatsAttribute(long maxFileSizeBytes, params string[] mimeTypes) : this(mimeTypes) =>
        _maxFileSizeBytes = maxFileSizeBytes;


    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Method intentionally left empty.
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Form.Files.Any(fileItem => !_allowedMimeTypes.Contains(fileItem.ContentType)))
        {
            context.Result = new UnsupportedMediaTypeResult();
        }
        else if (_maxFileSizeBytes > 0 &&
                    context.HttpContext.Request.Form.Files.Any(fileItem => fileItem.Length > _maxFileSizeBytes))
        {
            context.Result = new BadRequestObjectResult("The file exceeded allowed maximum size.");
        }
    }
}
