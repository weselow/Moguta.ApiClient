using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON строку или число в C# decimal.
/// Обрабатывает случаи, когда API может возвращать числовые значения как строки.
/// </summary>
public class StringToDecimalConverter : JsonConverter<decimal>
{
    /// <summary>
    /// Читает и преобразует JSON в decimal.
    /// </summary>
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                // Пустая или null строка преобразуется в 0
                return default; // 0m
            }
            // Используем InvariantCulture для надежной обработки '.' как разделителя.
            // NumberStyles.Any допускает пробелы, знаки и т.д.
            if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }
            // Выбрасываем исключение при неудачном парсинге
            throw new JsonException($"Не удалось преобразовать строку '{stringValue}' в Decimal.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal();
        }

        // Обработка null для non-nullable типа
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException($"Невозможно преобразовать null в non-nullable Decimal.");
        }

        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге Decimal.");
    }

    /// <summary>
    /// Записывает C# decimal как стандартное JSON число.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}