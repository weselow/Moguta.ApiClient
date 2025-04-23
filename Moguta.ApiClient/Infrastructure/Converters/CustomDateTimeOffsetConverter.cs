using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON строку формата "yyyy-MM-dd HH:mm:ss" или "0000-00-00 00:00:00" в DateTimeOffset?.
/// "0000-00-00 00:00:00" трактуется как null.
/// </summary>
public class CustomDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    private const string ExpectedFormat = "yyyy-MM-dd HH:mm:ss";
    private const string ZeroDateString = "0000-00-00 00:00:00"; // Нулевая дата от Moguta

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string? dateString = reader.GetString();

            // Если строка пустая или нулевая дата, возвращаем null
            if (string.IsNullOrWhiteSpace(dateString) || dateString == ZeroDateString)
            {
                return null; // <--- ДОБАВЛЕНО УСЛОВИЕ ДЛЯ НУЛЕВОЙ ДАТЫ
            }

            // Пытаемся распарсить в ожидаемом формате "yyyy-MM-dd HH:mm:ss"
            if (DateTimeOffset.TryParseExact(dateString, ExpectedFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTimeOffset result))
            {
                return result;
            }
            // Fallback на стандартный парсер ISO 8601
            else if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
            else
            {
                // Если не удалось распарсить и это не нулевая дата - ошибка
                throw new JsonException($"Не удалось преобразовать строку '{dateString}' в DateTimeOffset?. Ожидаемый формат: '{ExpectedFormat}', '{ZeroDateString}' или ISO 8601.");
            }
        }

        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге DateTimeOffset?.");
    }

    // Метод Write остается без изменений
    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("o")); // Записываем в ISO 8601
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}