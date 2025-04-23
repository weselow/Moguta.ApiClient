using System.Text.Json;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

// Подключаем наши конвертеры

namespace Moguta.ApiClient.Infrastructure;

/// <summary>
/// Вспомогательный класс для настроек и операций сериализации/десериализации JSON.
/// </summary>
internal static class SerializationHelper
{
    /// <summary>
    /// Настройки JsonSerializer по умолчанию для взаимодействия с Moguta API.
    /// </summary>
    public static JsonSerializerOptions DefaultJsonSerializerOptions { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            // Использовать snake_case для имен свойств (например, "user_email" вместо "UserEmail")
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            // Игнорировать свойства со значением null при сериализации
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // Разрешить чтение комментариев в JSON (хотя API их не использует)
            ReadCommentHandling = JsonCommentHandling.Skip,
            // Разрешить висячие запятые в JSON (хотя API их не использует)
            AllowTrailingCommas = true,
            // Добавляем наши кастомные конвертеры
            Converters = {
                new IntToBoolConverter(),
                new StringToDecimalConverter(),
                new StringToLongConverter(),
                new StringToNullableDecimalConverter(),
                new StringToIntConverter(),
                new StringToNullableIntConverter(),
                new StringToNullableLongConverter(),
                new CustomDateTimeOffsetConverter(),
                new OrderAddressConverter(),
                new OrderYurInfoConverter(),
                new CustomDateOnlyConverter()
                // new RuDateConverter(), // Раскомментировать, если нужен конвертер для дат dd.MM.yyyy
                // Добавить другие конвертеры при необходимости
            }
        };
        return options;
    }


    /// <summary>
    /// Сериализует объект в JSON строку, используя настройки по умолчанию.
    /// </summary>
    /// <typeparam name="T">Тип объекта.</typeparam>
    /// <param name="value">Объект для сериализации.</param>
    /// <returns>JSON строка.</returns>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, DefaultJsonSerializerOptions);
    }

    /// <summary>
    /// Десериализует JSON строку в объект указанного типа, используя настройки по умолчанию.
    /// </summary>
    /// <typeparam name="T">Тип объекта для десериализации.</typeparam>
    /// <param name="json">JSON строка.</param>
    /// <returns>Десериализованный объект или null, если строка пуста или null.</returns>
    /// <exception cref="JsonException">Выбрасывается, если JSON некорректен или не может быть преобразован в тип T.</exception>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default; // Возвращаем null для ссылочных типов или default для value-типов
        }
        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultJsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            // Добавляем часть JSON в сообщение об ошибке для облегчения отладки
            string snippet = json.Length > 500 ? json.Substring(0, 500) + "..." : json;
            throw new JsonException($"Ошибка десериализации JSON: {ex.Message}. JSON (начало): {snippet}", ex);
        }
    }
}