using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `deleteCategory`.
/// </summary>
public class DeleteCategoryRequestParams
{
    /// <summary>
    /// Получает или задает список ID категорий для удаления.
    /// Внимание: Документация API использует ключ "category", а не "categories".
    /// </summary>
    [JsonPropertyName("category")] // Используем "category" согласно документации
    public List<long> CategoryIds { get; set; } = [];
}