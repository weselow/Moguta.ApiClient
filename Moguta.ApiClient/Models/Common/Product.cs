using System.Text.Json;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;
using Moguta.ApiClient.Models.Common;

public class Product
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringToNullableLongConverter))] // Используем Nullable конвертер т.к. ID может быть null при создании
    public long? Id { get; set; }

    [JsonPropertyName("sort")]
    [JsonConverter(typeof(StringToNullableIntConverter))] // Может быть строкой или null? Добавим Nullable
    public int? Sort { get; set; }

    [JsonPropertyName("cat_id")]
    [JsonConverter(typeof(StringToLongConverter))] // Категория обязательна
    public long CatId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка
    public decimal Price { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка "-1"
    public decimal Count { get; set; }

    [JsonPropertyName("activity")]
    [JsonConverter(typeof(IntToBoolConverter))] // Строка "1"
    public bool Activity { get; set; } = true;

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("meta_keywords")]
    public string? MetaKeywords { get; set; }

    [JsonPropertyName("meta_desc")]
    public string? MetaDesc { get; set; }

    [JsonPropertyName("old_price")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Может быть пустой строкой
    public decimal? OldPrice { get; set; }

    [JsonPropertyName("recommend")]
    [JsonConverter(typeof(IntToBoolConverter))] // Строка "0"
    public bool? Recommend { get; set; } // Nullable bool

    [JsonPropertyName("new")]
    [JsonConverter(typeof(IntToBoolConverter))] // Строка "0"
    public bool? New { get; set; } // Nullable bool

    [JsonPropertyName("related")]
    public string? Related { get; set; }

    [JsonPropertyName("inside_cat")]
    public string? InsideCat { get; set; }

    [JsonPropertyName("1c_id")]
    public string? ExternalId1C { get; set; } // Строка или null

    [JsonPropertyName("weight")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Строка "1" или "0" или null?
    public decimal? Weight { get; set; }

    [JsonPropertyName("link_electro")]
    public string? LinkElectro { get; set; } // null

    [JsonPropertyName("currency_iso")]
    public string? CurrencyIso { get; set; } // "RUR"

    [JsonPropertyName("price_course")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка
    public decimal PriceCourse { get; set; } // Сделаем non-nullable, т.к. всегда есть

    [JsonPropertyName("image_title")]
    public string? ImageTitle { get; set; }

    [JsonPropertyName("image_alt")]
    public string? ImageAlt { get; set; }

    [JsonPropertyName("yml_sales_notes")]
    public string? YmlSalesNotes { get; set; } // null

    [JsonPropertyName("count_buy")]
    [JsonConverter(typeof(StringToLongConverter))] // Строка "0"
    public long CountBuy { get; set; }

    [JsonPropertyName("system_set")] // Непонятное поле, пока как строка
    public string? SystemSet { get; set; }

    [JsonPropertyName("related_cat")]
    public string? RelatedCat { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset? LastUpdated { get; set; } // Стандартный парсер должен справиться с "yyyy-MM-dd HH:mm:ss"

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("weight_unit")]
    public string? WeightUnit { get; set; } // null

    [JsonPropertyName("multiplicity")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка "1"
    public decimal Multiplicity { get; set; }

    [JsonPropertyName("storage_count")] // Неясно, число или строка
    [JsonConverter(typeof(StringToDecimalConverter))] // Предположим, что строка как и count
    public decimal? StorageCount { get; set; }

    [JsonPropertyName("variants")]
    public List<Variant>? Variants { get; set; }

    [JsonPropertyName("property")]
    public List<Property>? Property { get; set; }

    // Убираем поля, которых нет в ответе: category_url, images, yml, opf_*
    [JsonExtensionData] // Убираем OptionalFields, т.к. нет opf_*
    public Dictionary<string, JsonElement>? OptionalFields { get; set; }
}