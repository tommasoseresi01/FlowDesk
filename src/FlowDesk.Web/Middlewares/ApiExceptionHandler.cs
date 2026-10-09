using FlowDesk.Application.Exceptions;
using FlowDesk.Application.Models.Dtos;
using FlowDesk.Application.Models.Responses;
using Microsoft.AspNetCore.Diagnostics;

namespace FlowDesk.Web.Middlewares;

// Gestore globale delle eccezioni: i controller non hanno try/catch.
// Ogni eccezione diventa l'envelope standard con il codice HTTP giusto;
// i dettagli interni non escono mai dal server.
public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private const string ForbiddenMessage = "Non hai i permessi per eseguire questa operazione.";
    private const string UnexpectedMessage = "Si è verificato un errore imprevisto.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, errors) = exception switch
        {
            ApplicationValidationException validation =>
                (StatusCodes.Status422UnprocessableEntity, validation.Errors.ToList()),
            NotFoundException notFound =>
                (StatusCodes.Status404NotFound, [ToError(notFound.Message)]),
            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, [ToError(ForbiddenMessage)]),
            _ =>
                (StatusCodes.Status500InternalServerError, [ToError(UnexpectedMessage)])
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        var response = new ErrorResponse();
        response.WithErrors(errors);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private static ApplicationErrorDto ToError(string message) => new() { Message = message };
}
