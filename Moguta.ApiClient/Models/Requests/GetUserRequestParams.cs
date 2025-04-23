using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `getUsers`.
/// Позволяет указать email адреса или параметры пагинации. Следует использовать только одну группу параметров.
/// </summary>
public class GetUserRequestParams
{
    // Вариант 1: Пагинация
    /// <summary>
    /// Получает или задает номер страницы для выгрузки.
    /// Используется совместно с <see cref="Count"/>.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Получает или задает количество пользователей на странице. Максимум 250 согласно документации.
    /// Используется совместно с <see cref="Page"/>.
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    // Вариант 2: По Email
    /// <summary>
    /// Получает или задает список email адресов пользователей для выгрузки.
    /// Исключает использование пагинации.
    /// </summary>
    [JsonPropertyName("email")]
    public List<string>? Emails { get; set; }
}