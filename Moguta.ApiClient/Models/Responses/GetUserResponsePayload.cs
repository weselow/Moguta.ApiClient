using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Responses;

/// <summary>
/// Представляет полезную нагрузку (payload) ответа для API метода 'getUsers'.
/// </summary>
public class GetUserResponsePayload
{
    /// <summary>
    /// Получает общее количество пользователей, соответствующих запросу (для пагинации).
    /// </summary>
    [JsonPropertyName("countUsers")]
    [JsonConverter(typeof(StringToLongConverter))] // API возвращает строку
    public long TotalCount { get; set; }

    /// <summary>
    /// Получает список пользователей для текущей страницы.
    /// </summary>
    [JsonPropertyName("users")]
    public List<MogutaUser> Users { get; set; } = [];

    /// <summary>
    /// Получает номер текущей страницы (только при пагинации).
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Получает количество пользователей на странице (только при пагинации).
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }
}