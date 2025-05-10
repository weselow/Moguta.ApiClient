// В папке Infrastructure/Converters
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Infrastructure.Converters;

public class NullableListConverter<TItem> : JsonConverter<List<TItem>?>
{
    public override List<TItem>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null; // Возвращаем null, если в JSON явно null
        }

        // --- НАЧАЛО ИЗМЕНЕНИЙ ---
        if (reader.TokenType == JsonTokenType.String)
        {
            // Если это строка, проверяем, не пустая ли она
            string? stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                // Пустую строку трактуем как отсутствие данных = null (или пустой список)
                return null; // Или: return new List<TItem>();
            }
            // Если строка не пустая, это ошибка, т.к. ожидаем массив
            throw new JsonException($"Не удалось преобразовать JSON строку '{stringValue}' в List<{typeof(TItem).Name}>. Ожидался JSON массив или null.");
        }
        // --- КОНЕЦ ИЗМЕНЕНИЙ ---

        // Если это не null и не строка, то должен быть массив
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Неожиданный тип токена {reader.TokenType} при парсинге List<{typeof(TItem).Name}>. Ожидался JSON массив или null.");
        }

        // Используем стандартную десериализацию для списка, удалив себя из опций
        var optionsWithoutThis = new JsonSerializerOptions(options);
        bool removed = false;
        for (int i = 0; i < optionsWithoutThis.Converters.Count; i++)
        {
            // Сравниваем по полному имени типа, чтобы быть точнее
            if (optionsWithoutThis.Converters[i].GetType().FullName == this.GetType().FullName)
            {
                optionsWithoutThis.Converters.RemoveAt(i);
                removed = true;
                break;
            }
        }
        // Если наш конвертер не был в глобальных опциях, а только через атрибут,
        // то его и не будет в optionsWithoutThis.Converters, и удаление не нужно.

        return JsonSerializer.Deserialize<List<TItem>>(ref reader, optionsWithoutThis);
    }

    public override void Write(Utf8JsonWriter writer, List<TItem>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var optionsWithoutThis = new JsonSerializerOptions(options);
            bool removed = false;
            for (int i = 0; i < optionsWithoutThis.Converters.Count; i++)
            {
                if (optionsWithoutThis.Converters[i].GetType().FullName == this.GetType().FullName)
                {
                    optionsWithoutThis.Converters.RemoveAt(i);
                    removed = true;
                    break;
                }
            }
            JsonSerializer.Serialize(writer, value, optionsWithoutThis);
        }
    }
}