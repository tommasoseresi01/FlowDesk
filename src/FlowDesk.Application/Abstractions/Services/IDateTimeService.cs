namespace FlowDesk.Application.Abstractions.Services;

// Orologio dell'applicazione: restituisce sempre UTC e si sostituisce nei test.
public interface IDateTimeService
{
    DateTime Now();
}
