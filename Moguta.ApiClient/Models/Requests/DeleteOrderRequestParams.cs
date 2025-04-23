using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `deleteOrder`.
/// </summary>
public class DeleteOrderRequestParams
{
    /// <summary>
    /// Получает или задает список ID заказов для удаления.
    /// </summary>
    [JsonPropertyName("orders")] // Ключ "orders" согласно документации
    public List<long> OrderIds { get; set; } = [];
}