using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON строку, число или null в C# nullable decimal (decimal?).
/// Обрабатывает случаи, когда API может возвращать числовые значения как строки или пропускать их (null).
/// </summary>
public class StringToNullableDecimalConverter : JsonConverter<decimal?>
{
    /// <summary>
    /// Читает и преобразует JSON в decimal?.
    /// </summary>
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                // Пустая или null строка трактуется как null для nullable decimal
                return null;
            }
            // Используем InvariantCulture
            if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }
            throw new JsonException($"Не удалось преобразовать строку '{stringValue}' в Decimal?.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal();
        }

        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге Decimal?.");
    }

    /// <summary>
    /// Записывает C# decimal? как JSON число или null.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}