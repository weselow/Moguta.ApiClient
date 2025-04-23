using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON строку формата "yyyy-MM-dd" или пустую строку в DateOnly?.
/// Пустая строка или null трактуются как null.
/// </summary>
public class CustomDateOnlyConverter : JsonConverter<DateOnly?>
{
    private const string ExpectedFormat = "yyyy-MM-dd"; // Стандартный формат DateOnly

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string? dateString = reader.GetString();

            // Пустую строку считаем null
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null; // <-- ОБРАБОТКА ПУСТОЙ СТРОКИ
            }

            // Пытаемся распарсить в стандартном формате DateOnly
            if (DateOnly.TryParseExact(dateString, ExpectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result))
            {
                return result;
            }
            // Можно добавить другие форматы, если API их использует
            // else if (DateOnly.TryParseExact(dateString, "dd.MM.yyyy", ...)) { ... }
            else
            {
                throw new JsonException($"Не удалось преобразовать строку '{dateString}' в DateOnly?. Ожидаемый формат: '{ExpectedFormat}'.");
            }
        }

        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге DateOnly?.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            // Записываем в стандартном формате
            writer.WriteStringValue(value.Value.ToString(ExpectedFormat, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}