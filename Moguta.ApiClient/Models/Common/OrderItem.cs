using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Common;

/// <summary>
/// Представляет позицию (товар) внутри заказа.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Получает или задает ID товара.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringToLongConverter))] // API возвращает строку
    public long Id { get; set; }

    /// <summary>
    /// Получает или задает ID варианта товара (может быть null, если заказ без варианта).
    /// В JSON ответа поле называется "variant_id".
    /// </summary>
    [JsonPropertyName("variant_id")] // ИСПРАВЛЕНО ИМЯ ПОЛЯ
    [JsonConverter(typeof(StringToNullableLongConverter))] // API возвращает строку или null
    public long? VariantId { get; set; } // Тип изменен на Nullable Long

    /// <summary>
    /// Получает или задает название товара/варианта на момент заказа.
    /// В JSON ответа поле называется "name".
    /// </summary>
    [JsonPropertyName("name")] // ИСПРАВЛЕНО ИМЯ ПОЛЯ
    public string? Name { get; set; }

    /// <summary>
    /// Получает или задает URL товара.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Получает или задает артикул (SKU) товара/варианта.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Получает или задает цену за единицу этой позиции (с учетом скидок/купонов).
    /// </summary>
    [JsonPropertyName("price")]
    [JsonConverter(typeof(StringToDecimalConverter))] // API возвращает строку
    public decimal Price { get; set; }

    /// <summary>
    /// Получает или задает количество заказанных единиц этой позиции.
    /// </summary>
    [JsonPropertyName("count")]
    [JsonConverter(typeof(StringToDecimalConverter))] // API возвращает строку
    public decimal Count { get; set; }

    /// <summary>
    /// Получает или задает строку, описывающую выбранные свойства/опции для этой позиции (часто HTML).
    /// </summary>
    [JsonPropertyName("property")]
    public string? Property { get; set; }

    /// <summary>
    /// Получает или задает примененную скидку (обычно 0).
    /// </summary>
    [JsonPropertyName("discount")]
    [JsonConverter(typeof(StringToDecimalConverter))] // API возвращает строку "0"
    public decimal Discount { get; set; }

    /// <summary>
    /// Получает или задает полную цену за единицу (до скидок/купонов?).
    /// Обратите внимание на опечатку "fulPrice" в имени JSON поля.
    /// </summary>
    [JsonPropertyName("fulPrice")] // Опечатка из API
    [JsonConverter(typeof(StringToDecimalConverter))] // API возвращает строку
    public decimal FullPrice { get; set; }

    /// <summary>
    /// Получает или задает вес единицы товара.
    /// </summary>
    [JsonPropertyName("weight")]
    [JsonConverter(typeof(StringToDecimalConverter))] // API возвращает строку "0"
    public decimal Weight { get; set; }

    /// <summary>
    /// Получает или задает ISO код валюты (например, "RUR").
    /// </summary>
    [JsonPropertyName("currency_iso")]
    public string? CurrencyIso { get; set; }

    /// <summary>
    /// Получает или задает единицу измерения товара (например, "шт.").
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    /// <summary>
    /// Получает или задает идентификатор товара в 1С (если используется).
    /// </summary>
    [JsonPropertyName("1c_id")] // Имя свойства в C# не может начинаться с цифры, но JsonPropertyName может
    public string? ExternalId1C { get; set; } // Может быть пустой строкой

    // --- Поля, которые были в старой версии DTO, но отсутствуют в предоставленном JSON ответа ---
    // [JsonPropertyName("title")] // Заменено на "name"
    // public string? Title { get; set; }
    //
    // [JsonPropertyName("coupon")] // Отсутствует в JSON
    // public string? Coupon { get; set; }
    //
    // [JsonPropertyName("info")] // Отсутствует в JSON
    // public string? Info { get; set; }
    //
    // [JsonPropertyName("discSyst")] // Отсутствует в JSON
    // public string? DiscountSystemInfo { get; set; }
}