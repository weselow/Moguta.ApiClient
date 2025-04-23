using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Models.Common; // Для OrderYurInfo

namespace Moguta.ApiClient.Infrastructure.Converters;

/// <summary>
/// Конвертирует поле 'yur_info' из JSON.
/// Оно может быть либо булевым 'false', либо объектом OrderYurInfo.
/// </summary>
public class OrderYurInfoConverter : JsonConverter<OrderYurInfo?>
{
    public override OrderYurInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.False)
        {
            return null;
        }
        if (reader.TokenType == JsonTokenType.Null) // На всякий случай, если API вернет null
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Десериализуем объект стандартно, удалив себя из опций
            var optionsWithoutThis = new JsonSerializerOptions(options);
            optionsWithoutThis.Converters.Remove(this);
            return JsonSerializer.Deserialize<OrderYurInfo>(ref reader, optionsWithoutThis);
        }

        throw new JsonException($"Неожиданный тип токена {reader.TokenType} для поля yur_info. Ожидался объект или false.");
    }

    public override void Write(Utf8JsonWriter writer, OrderYurInfo? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            // При записи будем использовать null, т.к. false не несет информации
            writer.WriteNullValue();
        }
        else
        {
            var optionsWithoutThis = new JsonSerializerOptions(options);
            optionsWithoutThis.Converters.Remove(this);
            JsonSerializer.Serialize(writer, value, optionsWithoutThis);
        }
    }
}