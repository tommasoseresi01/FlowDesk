using FlowDesk.Application.Utils;
using Serilog.Context;

namespace FlowDesk.Web.Middlewares;

// Aggiunge lo username a tutte le righe di log della richiesta, dopo l'autenticazione.
public class LogUserNameMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var username = context.User.Identity is { IsAuthenticated: true }
            ? context.User.FindFirst(ClaimsNames.USER_EMAIL)?.Value ?? string.Empty
            : string.Empty;

        using (LogContext.PushProperty("Username", username))
        {
            await next(context);
        }
    }
}
