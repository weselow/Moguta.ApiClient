using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON строку или число в C# long (Int64).
/// Обрабатывает случаи, когда API может возвращать числовые ID как строки.
/// </summary>
public class StringToLongConverter : JsonConverter<long>
{
    /// <summary>
    /// Читает и преобразует JSON в long.
    /// </summary>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                // Пустая или null строка преобразуется в 0
                return default; // 0L
            }
            // Используем InvariantCulture, NumberStyles.Integer для целых чисел
            if (long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
            {
                return value;
            }
            throw new JsonException($"Не удалось преобразовать строку '{stringValue}' в Int64.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            // Проверяем, помещается ли число в диапазон Int64
            if (reader.TryGetInt64(out long value))
            {
                return value;
            }
            throw new JsonException("Число JSON вне диапазона для Int64.");
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException($"Невозможно преобразовать null в non-nullable Int64.");
        }

        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге Int64.");
    }

    /// <summary>
    /// Записывает C# long как JSON число.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}