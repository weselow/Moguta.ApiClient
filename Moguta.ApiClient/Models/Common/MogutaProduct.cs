using System.Text.Json;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Common;

public class MogutaProduct
{
    /// <summary>
    /// id товара
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringToNullableLongConverter))] // Используем Nullable конвертер т.к. ID может быть null при создании
    public long? Id { get; set; }


    [JsonPropertyName("sort")]
    [JsonConverter(typeof(StringToNullableIntConverter))] // Может быть строкой или null? Добавим Nullable
    public int? Sort { get; set; }

    /// <summary>
    /// id категории товара
    /// </summary>
    [JsonPropertyName("cat_id")]
    [JsonConverter(typeof(StringToLongConverter))] // Категория обязательна
    public long CatId { get; set; }

    /// <summary>
    /// название товара
    /// </summary>
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание товара
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// краткое описание товара
    /// </summary>
    [JsonPropertyName("short_description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ShortDescription { get; set; }




    /// <summary>
    /// последняя секция урла
    /// </summary>
    /// <remarks>
    /// <para>Пример: 'raspredelitelnyy-elektroshkaf'</para>
    /// <para>Пример: 'test-prod-create-35cc1c8c2e' - слеша нет на конце.</para>
    /// </remarks>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;



    /// <summary>
    /// артикул
    /// </summary>
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    /// <summary>
    /// количество
    /// </summary>
    [JsonPropertyName("count")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка "-1"
    public decimal Count { get; set; }

    /// <summary>
    /// видимость товара
    /// </summary>
    [JsonPropertyName("activity")]
    [JsonConverter(typeof(IntToBoolConverter))] // Строка "1"
    public bool Activity { get; set; } = true;


    #region SEO

    /// <summary>
    /// заголовок страницы
    /// </summary>
    [JsonPropertyName("meta_title")] public string? MetaTitle { get; set; }

    /// <summary>
    /// ключевые слова
    /// </summary>
    [JsonPropertyName("meta_keywords")]
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// мета описание
    /// </summary>
    [JsonPropertyName("meta_desc")] public string? MetaDesc { get; set; }

    #endregion

    #region Price

    /// <summary>
    /// старая цена
    /// </summary>
    [JsonPropertyName("old_price")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Может быть пустой строкой
    public decimal? OldPrice { get; set; }

    /// <summary>
    /// символьный код валюты
    /// </summary>
    [JsonPropertyName("currency_iso")] public string? CurrencyIso { get; set; } // "RUR"

    /// <summary>
    /// цена в валюте магазина
    /// </summary>
    [JsonPropertyName("price_course")]
    [JsonConverter(typeof(StringToDecimalConverter))]
    public decimal PriceCourse { get; set; }

    /// <summary>
    /// цена
    /// </summary>
    [JsonPropertyName("price")]
    [JsonConverter(typeof(StringToDecimalConverter))]
    public decimal Price { get; set; }

    #endregion

    #region Images

    [JsonPropertyName("image_title")] public string? ImageTitle { get; set; }

    [JsonPropertyName("image_alt")] public string? ImageAlt { get; set; }

    /// <summary>
    /// Cсылки на изображения, разделитель | .
    /// </summary>
    /// <remarks>Можно указать название файла с изображением,
    /// если он уже располагается в директории товара или указать ссылку на изображение
    /// и она будет загружена к товару
    /// (при загрузке иозбражений по ссылке, метод запроса к API должен быть POST)</remarks>
    [JsonPropertyName("image_url")] public string? ImageUrl { get; set; }

    #endregion



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

    /// <summary>
    /// вес
    /// </summary>
    [JsonPropertyName("weight")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Строка "1" или "0" или null?
    public decimal? Weight { get; set; }

    /// <summary>
    /// сcылка на электронный товар
    /// </summary>
    [JsonPropertyName("link_electro")] public string? LinkElectro { get; set; } // null


    [JsonPropertyName("yml_sales_notes")]
    public string? YmlSalesNotes { get; set; } // null

    [JsonPropertyName("count_buy")]
    [JsonConverter(typeof(StringToLongConverter))] // Строка "0"
    public long CountBuy { get; set; }

    [JsonPropertyName("system_set")] // Непонятное поле, пока как строка
    public string? SystemSet { get; set; }

    [JsonPropertyName("related_cat")]
    public string? RelatedCat { get; set; }



    [JsonPropertyName("last_updated")]
    public DateTimeOffset? LastUpdated { get; set; } // Стандартный парсер должен справиться с "yyyy-MM-dd HH:mm:ss"

    /// <summary>
    /// единица измерения
    /// </summary>
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("weight_unit")] public string? WeightUnit { get; set; } // null

    [JsonPropertyName("multiplicity")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Строка "1"
    public decimal Multiplicity { get; set; }

    [JsonPropertyName("storage_count")] // Неясно, число или строка
    [JsonConverter(typeof(StringToDecimalConverter))] // Предположим, что строка как и count
    public decimal? StorageCount { get; set; }

    [JsonPropertyName("variants")]
    public List<MogutaVariant>? Variants { get; set; }

    /// <summary>
    /// Характеристики (аттрибуты)
    /// </summary>
    [JsonPropertyName("property")]
    public List<MogutaProperty>? Property { get; set; }


    // Убираем поля, которых нет в ответе: category_url, images, yml, opf_*
    [JsonExtensionData] // Убираем OptionalFields, т.к. нет opf_*
    public Dictionary<string, JsonElement>? OptionalFields { get; set; }
}