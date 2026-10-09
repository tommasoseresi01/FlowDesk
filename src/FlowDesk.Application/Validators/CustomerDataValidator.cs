using FlowDesk.Application.Models.Requests.Customers;
using FluentValidation;

namespace FlowDesk.Application.Validators;

// Regole comuni a creazione e modifica. I messaggi sono mostrati all'utente, quindi in italiano.
// Il controllo di unicità della partita IVA sta nel servizio: richiede il database.
public abstract class CustomerDataValidator<T> : AbstractValidator<T>
    where T : ICustomerData
{
    protected CustomerDataValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty().WithMessage("La ragione sociale è obbligatoria.")
            .MaximumLength(200).WithMessage("La ragione sociale può avere al massimo 200 caratteri.");

        RuleFor(x => x.VatNumber)
            .NotEmpty().WithMessage("La partita IVA è obbligatoria.")
            .Matches(@"^\d{11}$").WithMessage("La partita IVA deve avere 11 cifre.");

        RuleFor(x => x.ContactName)
            .MaximumLength(120).WithMessage("Il referente può avere al massimo 120 caratteri.");

        RuleFor(x => x.Email)
            .MaximumLength(254).WithMessage("L'email può avere al massimo 254 caratteri.")
            // Stessa regola del form del frontend: serve almeno un punto nel dominio.
            .Matches(@"^\S+@\S+\.\S+$").WithMessage("L'indirizzo email non è valido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Il telefono può avere al massimo 30 caratteri.");
    }
}

public class CreateCustomerRequestValidator : CustomerDataValidator<CreateCustomerRequest>
{
}

public class EditCustomerRequestValidator : CustomerDataValidator<EditCustomerRequest>
{
    public EditCustomerRequestValidator()
    {
        RuleFor(x => x.IdCustomer)
            .GreaterThan(0).WithMessage("L'identificativo del cliente non è valido.");
    }
}
