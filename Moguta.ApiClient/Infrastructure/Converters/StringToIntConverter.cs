using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON строку или число в C# int (Int32).
/// </summary>
public class StringToIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue)) return default; // 0
            if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)) return value;
            throw new JsonException($"Не удалось преобразовать строку '{stringValue}' в Int32.");
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int value)) return value;
            throw new JsonException("Число JSON вне диапазона для Int32.");
        }
        if (reader.TokenType == JsonTokenType.Null) throw new JsonException($"Невозможно преобразовать null в non-nullable Int32.");
        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге Int32.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// Конвертирует JSON строку, число или null в C# nullable int (Int32?).
/// </summary>
public class StringToNullableIntConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue)) return null;
            if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)) return value;
            throw new JsonException($"Не удалось преобразовать строку '{stringValue}' в Int32?.");
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int value)) return value;
            throw new JsonException("Число JSON вне диапазона для Int32?.");
        }
        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге Int32?.");
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue) writer.WriteNumberValue(value.Value);
        else writer.WriteNullValue();
    }
}