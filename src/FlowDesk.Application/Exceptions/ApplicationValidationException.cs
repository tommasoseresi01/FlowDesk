using FlowDesk.Application.Models.Dtos;

namespace FlowDesk.Application.Exceptions;

// Regola di business violata: arriva al client come 422 con l'elenco degli errori per campo.
public class ApplicationValidationException : Exception
{
    public ApplicationValidationException(string field, string message)
        : this([new ApplicationErrorDto { Field = field, Message = message }])
    {
    }

    public ApplicationValidationException(IReadOnlyList<ApplicationErrorDto> errors)
        : base(errors.Count > 0 ? errors[0].Message : "Validation failed")
    {
        Errors = errors;
    }

    public IReadOnlyList<ApplicationErrorDto> Errors { get; }
}
