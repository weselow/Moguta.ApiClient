using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `deleteProduct`.
/// </summary>
public class DeleteProductRequestParams
{
    /// <summary>
    /// Получает или задает список ID товаров для удаления.
    /// </summary>
    [JsonPropertyName("products")] // Ключ "products" согласно документации
    public List<long> ProductIds { get; set; } = [];
}