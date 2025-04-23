using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

public class Variant
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringToLongConverter))] // Строка
    public long Id { get; set; } // Сделаем non-nullable, т.к. всегда есть у варианта

    [JsonPropertyName("product_id")]
    [JsonConverter(typeof(StringToLongConverter))] // Строка
    public long ProductId { get; set; } // Сделаем non-nullable

    [JsonPropertyName("title_variant")]
    public string? TitleVariant { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; } // Может быть null

    [JsonPropertyName("sort")]
    [JsonConverter(typeof(StringToIntConverter))] // Строка
    public int Sort { get; set; } // Сделаем non-nullable

    [JsonPropertyName("price")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка
    public decimal Price { get; set; }

    [JsonPropertyName("old_price")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Пустая строка
    public decimal? OldPrice { get; set; }

    [JsonPropertyName("count")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка "0" или "-1"
    public decimal Count { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("activity")]
    [JsonConverter(typeof(IntToBoolConverter))] // Строка "1"
    public bool Activity { get; set; } = true;

    [JsonPropertyName("weight")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка "0"
    public decimal Weight { get; set; } // Сделаем non-nullable, т.к. всегда "0" в примере

    [JsonPropertyName("currency_iso")]
    public string? CurrencyIso { get; set; } // "RUR"

    [JsonPropertyName("price_course")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка
    public decimal PriceCourse { get; set; } // Сделаем non-nullable

    [JsonPropertyName("1c_id")]
    public string? ExternalId1C { get; set; } // null

    [JsonPropertyName("color")]
    [JsonConverter(typeof(StringToNullableLongConverter))] // Строка
    public long? ColorId { get; set; }

    [JsonPropertyName("size")]
    [JsonConverter(typeof(StringToNullableLongConverter))] // Строка
    public long? SizeId { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset? LastUpdated { get; set; }
}