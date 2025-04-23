using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует JSON значение (число 0/1 или строку "0"/"1") в C# bool.
/// Используется для полей типа activity, blocked, invisible и т.д.
/// </summary>
public class IntToBoolConverter : JsonConverter<bool>
{
    /// <summary>
    /// Читает и преобразует JSON в bool.
    /// </summary>
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            // Если 0 - false, любое другое число - true
            return reader.GetInt32() != 0;
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            // Пытаемся распарсить строку как число
            return int.TryParse(reader.GetString(), out int val) && val != 0;
        }
        // Напрямую обрабатываем true/false, если API вдруг их вернет
        if (reader.TokenType == JsonTokenType.True) return true;
        if (reader.TokenType == JsonTokenType.False) return false;

        // Выбрасываем исключение для неожиданных типов
        throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге bool из числа/строки.");
    }

    /// <summary>
    /// Записывает C# bool как JSON число (0 или 1).
    /// </summary>
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value ? 1 : 0);
    }
}