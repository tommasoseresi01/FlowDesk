using FlowDesk.Application.Models.Dtos;
using FlowDesk.Application.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Web.Factories;

// Trasforma gli errori di validazione di MVC (FluentValidation, JSON non valido) nell'envelope standard con HTTP 400.
public class BadRequestResultFactory(ActionContext context) : BadRequestObjectResult(Build(context))
{
    private const string InvalidDataMessage = "I dati inviati non sono validi.";

    private static ErrorResponse Build(ActionContext context)
    {
        var errors = new List<ApplicationErrorDto>();

        foreach (var (key, entry) in context.ModelState)
        {
            foreach (var error in entry.Errors)
            {
                errors.Add(new ApplicationErrorDto
                {
                    // I campi di FluentValidation sono i nomi delle proprietà (es. "VatNumber"):
                    // il frontend li riporta sul campo del form.
                    Field = key.StartsWith("$.", StringComparison.Ordinal) ? string.Empty : key,

                    // Gli errori di lettura del JSON hanno testi tecnici: all'utente se ne mostra uno generico.
                    Message = error.Exception is not null || string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? InvalidDataMessage
                        : error.ErrorMessage
                });
            }
        }

        if (errors.Count == 0)
        {
            errors.Add(new ApplicationErrorDto { Message = InvalidDataMessage });
        }

        var response = new ErrorResponse();
        response.WithErrors(errors);
        return response;
    }
}
