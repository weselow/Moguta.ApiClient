using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `getOrder`.
/// Позволяет указать ID, номера заказов, email клиентов или параметры пагинации. Следует использовать только одну группу параметров.
/// </summary>
public class GetOrderRequestParams
{
    // Вариант 1: Пагинация
    /// <summary>
    /// Получает или задает номер страницы для выгрузки.
    /// Используется совместно с <see cref="Count"/>.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Получает или задает количество заказов на странице. Максимум 250 согласно документации.
    /// Используется совместно с <see cref="Page"/>.
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    // Вариант 2: По внутреннему ID Заказа
    /// <summary>
    /// Получает или задает список внутренних ID заказов для выгрузки.
    /// Исключает использование других фильтров.
    /// </summary>
    [JsonPropertyName("id")]
    public List<long>? Ids { get; set; }

    // Вариант 3: По публичному номеру заказа (например, "M-12345")
    /// <summary>
    /// Получает или задает список публичных номеров заказов для выгрузки.
    /// Исключает использование других фильтров.
    /// </summary>
    [JsonPropertyName("number")]
    public List<string>? Numbers { get; set; }

    // Вариант 4: По Email клиента
    /// <summary>
    /// Получает или задает список email адресов клиентов, чьи заказы нужно выгрузить.
    /// Исключает использование других фильтров.
    /// </summary>
    [JsonPropertyName("email")]
    public List<string>? Emails { get; set; }
}