using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `importCategory`.
/// </summary>
public class ImportCategoryRequestParams
{
    /// <summary>
    /// Получает или задает список категорий для импорта (создания или обновления).
    /// Рекомендуемый размер пакета - до 100 записей.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<MogutaCategory> Categories { get; set; } = [];

    // Флаг 'enableUpdate' не показан в примерах для категорий,
    // обновление, вероятно, происходит неявно при наличии ID.
}