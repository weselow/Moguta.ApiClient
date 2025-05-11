using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Common;

/// <summary>
/// Представляет структурированные детали адреса внутри заказа.
/// Основано на поле 'address_parts' в примере getOrder.
/// </summary>
public class MogutaOrderAddress
{
    /// <summary>
    /// Получает или задает почтовый индекс.
    /// </summary>
    [JsonPropertyName("index")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Получает или задает страну.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// Получает или задает регион/область/республику.
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>
    /// Получает или задает город.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Получает или задает улицу.
    /// </summary>
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    /// <summary>
    /// Получает или задает номер дома.
    /// </summary>
    [JsonPropertyName("house")]
    public string? House { get; set; }

    /// <summary>
    /// Получает или задает номер квартиры/офиса.
    /// </summary>
    [JsonPropertyName("flat")]
    public string? Flat { get; set; }
}