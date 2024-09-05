using System.Net;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using School.Shared.Core.Abstractions.Exceptions;
using School.Shared.Core.Abstractions.Options;

namespace School.Shared.Core.Filters;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ServiceOptions _options;

    public ExceptionFilter(IOptions<ServiceOptions> options)
    {
        _options = options.Value;
    }

    public void OnException(ExceptionContext ctx)
    {
        switch (ctx.Exception)
        {
            case ValidationException exception:
            {
                ValidationResult result = new(exception.Errors);
                result.AddToModelState(ctx.ModelState, null);

                ctx.Result = new BadRequestObjectResult(ctx.ModelState);
                break;
            }

            case AggregateException ae when ae.InnerExceptions.Any(x => x is TaskCanceledException or OperationCanceledException):
            case TaskCanceledException:
            case OperationCanceledException:
            {
                ctx.HttpContext.Response.ContentType = "application/json";
                ctx.HttpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                ctx.Result = GetContextResult(ctx);
                break;
            }

            case NotFoundException:
            {
                ctx.HttpContext.Response.ContentType = "applicaiton/json";
                ctx.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                ctx.Result = GetContextResult(ctx);
                break;
            }

            case BadRequestException:
            {
                ctx.HttpContext.Response.ContentType = "applicaiton/json";
                ctx.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                ctx.Result = GetContextResult(ctx);
                break;
            }

            case ForbiddenException:
            {
                ctx.HttpContext.Response.ContentType = "applicaiton/json";
                ctx.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                ctx.Result = GetContextResult(ctx);
                break;
            }

            case TooManyRequestsException:
            {
                ctx.HttpContext.Response.ContentType = "applicaiton/json";
                ctx.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                ctx.Result = GetContextResult(ctx);
                break;
            }

            case UnauthorizedException:
            {
                ctx.HttpContext.Response.ContentType = "applicaiton/json";
                ctx.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                ctx.Result = GetContextResult(ctx);
                break;
            }

            default:
            {
                ctx.HttpContext.Response.ContentType = "applicaiton/json";
                ctx.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ctx.Result = GetContextResult(ctx);
                break;
            }
        }
    }

    private IActionResult? GetContextResult(ExceptionContext ctx)
    {
        // In development, we want as much information as possible.
        if (_options.Debug)
        {
            return new JsonResult(new
            {
                Message = ctx.Exception.InnerException is not null
                    ? ctx.Exception.InnerException.Message
                    : ctx.Exception.Message,
                
                Stacktrace = ctx.Exception.StackTrace
            });
        }

        // If not in development we need to limit information for security concerns.
        if (ctx.Exception.GetType().IsAssignableFrom(typeof(ServiceException)))
        {
            return new JsonResult(new
            {
                Title = ((HttpStatusCode)ctx.HttpContext.Response.StatusCode).ToString(),
                Status = ctx.HttpContext.Response.StatusCode,
                Message = ctx.Exception.InnerException is not null
                    ? ctx.Exception.InnerException.Message
                    : ctx.Exception.Message
            });
        }

        return new StatusCodeResult(ctx.HttpContext.Response.StatusCode);
    }
}