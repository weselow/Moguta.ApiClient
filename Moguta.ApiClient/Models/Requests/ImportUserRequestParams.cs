using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `importUsers`.
/// </summary>
public class ImportUserRequestParams
{
    /// <summary>
    /// Получает или задает список пользователей для импорта (создания или обновления).
    /// Рекомендуемый размер пакета - до 100 записей.
    /// </summary>
    [JsonPropertyName("users")]
    public List<MogutaUser> Users { get; set; } = [];

    /// <summary>
    /// Получает или задает значение, указывающее, следует ли обновлять существующих пользователей при совпадении (по email).
    /// Если <c>true</c> - обновлять, если <c>false</c> - только создавать новых.
    /// Если <c>null</c> - используется поведение API по умолчанию (вероятно, true).
    /// </summary>
    [JsonPropertyName("enableUpdate")]
    public bool? EnableUpdate { get; set; } // Пример в документации показывает true
}