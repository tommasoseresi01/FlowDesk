using System.Text.Json;
using FlowDesk.Application.Converters;

namespace FlowDesk.UnitTests.Converters;

public class MyDateTimeConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new MyDateTimeConverter() }
    };

    private record Sample(DateTime Date, DateTime? Optional);

    [Fact]
    public void Dates_are_written_in_the_format_the_frontend_reads()
    {
        var json = JsonSerializer.Serialize(new Sample(new DateTime(2026, 10, 9, 14, 5, 7), null), Options);

        Assert.Contains("\"Date\":\"2026-10-09T14:05:07\"", json);
        Assert.Contains("\"Optional\":null", json);
    }

    [Fact]
    public void A_nullable_date_uses_the_same_format()
    {
        var json = JsonSerializer.Serialize(
            new Sample(DateTime.UnixEpoch, new DateTime(2026, 1, 31, 23, 59, 1)),
            Options);

        Assert.Contains("\"Optional\":\"2026-01-31T23:59:01\"", json);
    }

    [Fact]
    public void A_date_without_offset_is_read_back_unchanged()
    {
        var sample = JsonSerializer.Deserialize<Sample>(
            "{\"Date\":\"2026-10-09T14:05:07\",\"Optional\":null}",
            Options);

        Assert.NotNull(sample);
        Assert.Equal(new DateTime(2026, 10, 9, 14, 5, 7), sample.Date);
    }

    [Fact]
    public void An_invalid_date_is_a_json_error()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Sample>("{\"Date\":\"not a date\",\"Optional\":null}", Options));
    }
}
