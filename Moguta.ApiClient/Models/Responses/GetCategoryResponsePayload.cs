using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters; // Для StringToLongConverter

namespace Moguta.ApiClient.Models.Responses;

/// <summary>
/// Представляет полезную нагрузку (payload) ответа для API метода 'getCategory'.
/// </summary>
public class GetCategoryResponsePayload
{
    /// <summary>
    /// Получает общее количество категорий, соответствующих запросу (для пагинации).
    /// </summary>
    [JsonPropertyName("countCategory")]
    [JsonConverter(typeof(StringToLongConverter))] // API возвращает строку
    public long TotalCount { get; set; }

    /// <summary>
    /// Получает список категорий для текущей страницы.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<MogutaCategory> Categories { get; set; } = [];

    /// <summary>
    /// Получает номер текущей страницы (только при пагинации).
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; } // Может отсутствовать при запросе по ID/URL

    /// <summary>
    /// Получает количество категорий на странице (только при пагинации).
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; } // Может отсутствовать при запросе по ID/URL
}