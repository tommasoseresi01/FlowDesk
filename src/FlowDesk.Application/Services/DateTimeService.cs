using FlowDesk.Application.Abstractions.Services;

namespace FlowDesk.Application.Services;

public class DateTimeService(TimeProvider timeProvider) : IDateTimeService
{
    public DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
