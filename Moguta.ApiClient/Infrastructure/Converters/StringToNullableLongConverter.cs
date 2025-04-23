using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON строку, число или null в C# nullable long (Int64?).
/// </summary>
public class StringToNullableLongConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            if (long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value)) return value;
            throw new JsonException($"Не удалось преобразовать строку '{stringValue}' в Int64?.");
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out long value)) return value;
            throw new JsonException("Число JSON вне диапазона для Int64?.");
        }
        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге Int64?.");
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteNumberValue(value.Value);
        else writer.WriteNullValue();
    }
}