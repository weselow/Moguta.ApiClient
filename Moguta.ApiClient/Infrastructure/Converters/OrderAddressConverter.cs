// В папке Infrastructure/Converters
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Models.Common; // Для OrderAddress

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует поле 'address_parts' из JSON.
/// Оно может быть либо булевым 'false', либо объектом OrderAddress.
/// </summary>
public class OrderAddressConverter : JsonConverter<MogutaOrderAddress?>
{
    public override MogutaOrderAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Проверяем тип токена
        if (reader.TokenType == JsonTokenType.False)
        {
            // Если это булево false, возвращаем null
            return null;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Если это объект, десериализуем его стандартным образом как OrderAddress
            // Важно: Нужно использовать копию options без этого конвертера, чтобы избежать рекурсии
            var optionsWithoutThis = new JsonSerializerOptions(options);
            optionsWithoutThis.Converters.Remove(this); // Удаляем себя из копии опций

            return JsonSerializer.Deserialize<MogutaOrderAddress>(ref reader, optionsWithoutThis);
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // Если тип токена другой, выбрасываем исключение
        throw new JsonException($"Неожиданный тип токена {reader.TokenType} для поля address_parts. Ожидался объект или false.");
    }

    public override void Write(Utf8JsonWriter writer, MogutaOrderAddress? value, JsonSerializerOptions options)
    {
        // При записи мы всегда будем записывать либо объект, либо null
        if (value == null)
        {
            // Записывать null или false? API, вероятно, ожидает null или отсутствие поля. Запишем null.
            writer.WriteNullValue();
            // Если API требует именно false, раскомментируйте: writer.WriteBooleanValue(false);
        }
        else
        {
            // Важно: Используем копию options без этого конвертера
            var optionsWithoutThis = new JsonSerializerOptions(options);
            optionsWithoutThis.Converters.Remove(this);
            JsonSerializer.Serialize(writer, value, optionsWithoutThis);
        }
    }
}