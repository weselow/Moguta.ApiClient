using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `getCategory`.
/// Позволяет указать ID, URL или параметры пагинации. Следует использовать только одну группу параметров.
/// </summary>
public class GetCategoryRequestParams
{
    // Вариант 1: Пагинация
    /// <summary>
    /// Получает или задает номер страницы для выгрузки.
    /// Используется совместно с <see cref="Count"/>.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Получает или задает количество категорий на странице. Максимум 250 согласно документации.
    /// Используется совместно с <see cref="Page"/>.
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    // Вариант 2: По ID
    /// <summary>
    /// Получает или задает список ID категорий для выгрузки.
    /// Исключает использование пагинации или фильтрации по URL.
    /// </summary>
    [JsonPropertyName("id")]
    public List<long>? Ids { get; set; }

    // Вариант 3: По URL (последняя часть)
    /// <summary>
    /// Получает или задает список URL-псевдонимов (slug) категорий для выгрузки.
    /// Исключает использование пагинации или фильтрации по ID.
    /// </summary>
    [JsonPropertyName("url")]
    public List<string>? Urls { get; set; }

    // Примечание: Документация не упоминает флаги вроде 'includeProducts' или 'includeSubcategories'.
    // Добавить при необходимости, если API их поддерживает.
}