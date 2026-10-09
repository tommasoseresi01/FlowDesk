using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowDesk.Application.Converters;

// Le date viaggiano come "yyyy-MM-ddTHH:mm:ss", senza fuso: sono sempre in UTC.
// È lo stesso formato che il frontend legge con parseDate.
public class MyDateTimeConverter : JsonConverter<DateTime>
{
    public const string Format = "yyyy-MM-dd'T'HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();
        if (!DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var value))
        {
            throw new JsonException($"Invalid date: {text}");
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
