using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `importProduct`.
/// </summary>
public class ImportProductRequestParams
{
    /// <summary>
    /// Получает или задает список товаров для импорта (создания или обновления).
    /// Рекомендуемый размер пакета - до 100 записей.
    /// </summary>
    [JsonPropertyName("products")]
    public List<MogutaProduct> Products { get; set; } = [];

    // Флаг 'enableUpdate' для товаров не документирован явно в примерах API,
    // но аналогичный флаг есть для пользователей.
    // Оставляем его закомментированным, обновление, вероятно, неявно по ID.
    // [JsonPropertyName("enableUpdate")]
    // public bool? EnableUpdate { get; set; }
}