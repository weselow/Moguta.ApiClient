using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `getProduct`.
/// Позволяет указать ID, артикулы, названия или параметры пагинации. Следует использовать только одну группу параметров.
/// </summary>
public class GetMogutaProductRequestParams
{
    // Вариант 1: Пагинация
    /// <summary>
    /// Получает или задает номер страницы для выгрузки.
    /// Используется совместно с <see cref="Count"/>.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Получает или задает количество товаров на странице. Максимум 100 согласно документации.
    /// Используется совместно с <see cref="Page"/>.
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    // Вариант 2: По ID Товара
    /// <summary>
    /// Получает или задает список ID товаров для выгрузки.
    /// Исключает использование других фильтров.
    /// </summary>
    [JsonPropertyName("id")]
    public List<long>? Ids { get; set; }

    // Вариант 3: По Артикулу (SKU)
    /// <summary>
    /// Получает или задает список артикулов (SKU) товаров для выгрузки.
    /// Исключает использование других фильтров.
    /// </summary>
    [JsonPropertyName("code")]
    public List<string>? Codes { get; set; }

    // Вариант 4: По Названию Товара
    /// <summary>
    /// Получает или задает список названий товаров для выгрузки.
    /// Исключает использование других фильтров.
    /// </summary>
    [JsonPropertyName("title")]
    public List<string>? Titles { get; set; }

    /// <summary>
    /// Получает или задает значение, указывающее, следует ли включать информацию о вариантах товара в ответ.
    /// По умолчанию <c>false</c>.
    /// </summary>
    [JsonPropertyName("variants")]
    public bool? IncludeVariants { get; set; } // bool? для игнорирования при null, если API ожидает строку "true"/"false", нужен конвертер

    /// <summary>
    /// Получает или задает значение, указывающее, следует ли включать информацию о характеристиках товара в ответ.
    /// По умолчанию <c>false</c>.
    /// </summary>
    [JsonPropertyName("property")]
    public bool? IncludeProperties { get; set; } // bool? для игнорирования при null, если API ожидает строку "true"/"false", нужен конвертер
}