using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Responses;

/// <summary>
/// Представляет полезную нагрузку (payload) ответа для API метода 'getOrder'.
/// </summary>
public class GetOrderResponsePayload
{
    /// <summary>
    /// Получает общее количество заказов, соответствующих запросу (для пагинации).
    /// </summary>
    [JsonPropertyName("countOrder")] // Имя из JSON
    [JsonConverter(typeof(StringToLongConverter))] // API возвращает строку
    public long TotalCount { get; set; }

    /// <summary>
    /// Получает список заказов для текущей страницы.
    /// </summary>
    [JsonPropertyName("orders")] // Имя из JSON
    public List<MogutaOrder> Orders { get; set; } = [];

    /// <summary>
    /// Получает номер текущей страницы (только при пагинации).
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Получает количество заказов на странице (только при пагинации).
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }
}