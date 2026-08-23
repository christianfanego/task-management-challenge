using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskManagement.Api.Json;

public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTime = reader.GetDateTime();
        // Always treat deserialized dates as UTC (date-only strings like "2025-09-25" have no timezone)
        var result = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        Console.WriteLine($"[UTC-Read] Input: {dateTime} Kind={dateTime.Kind} Output: {result} Kind={result.Kind}");
        return result;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Ensure the output is always UTC, never convert an already-UTC value
        var utc = value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value.ToUniversalTime(), DateTimeKind.Utc);
        var iso = utc.ToString("O");
        Console.WriteLine($"[UTC-Write] Input: {value} Kind={value.Kind} Output: {iso}");
        writer.WriteStringValue(iso);
    }
}

public class NullableUtcDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var dateTime = reader.GetDateTime();
        var result = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        Console.WriteLine($"[Nullable-Read] Input: {dateTime} Kind={dateTime.Kind} Output: {result} Kind={result.Kind}");
        return result;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }
        var utc = value.Value.Kind == DateTimeKind.Utc ? value.Value : DateTime.SpecifyKind(value.Value.ToUniversalTime(), DateTimeKind.Utc);
        var iso = utc.ToString("O");
        Console.WriteLine($"[Nullable-Write] Input: {value.Value} Kind={value.Value.Kind} Output: {iso}");
        writer.WriteStringValue(iso);
    }
}
