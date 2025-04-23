using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Common;

/// <summary>
/// Представляет информацию о юридическом лице, связанную с заказом.
/// Основано на поле 'yur_info' в примере getOrder.
/// </summary>
public class OrderYurInfo
{
    /// <summary>
    /// Получает или задает Email (вероятно, из профиля пользователя).
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Получает или задает Имя (контактное лицо? название компании? неоднозначно).
    /// </summary>
    [JsonPropertyName("name")]
    public string? ContactPersonName { get; set; }

    /// <summary>
    /// Получает или задает Адрес (физический? почтовый? неоднозначно).
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Получает или задает Телефон.
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Получает или задает ИНН (Идентификационный номер налогоплательщика).
    /// </summary>
    [JsonPropertyName("inn")]
    public string? Inn { get; set; }

    /// <summary>
    /// Получает или задает КПП (Код причины постановки на учет).
    /// </summary>
    [JsonPropertyName("kpp")]
    public string? Kpp { get; set; }

    /// <summary>
    /// Получает или задает официальное наименование юридического лица.
    /// </summary>
    [JsonPropertyName("nameyur")]
    public string? LegalName { get; set; }

    /// <summary>
    /// Получает или задает юридический адрес (опечатка 'adress' из примера API).
    /// </summary>
    [JsonPropertyName("adress")] // Опечатка из API
    public string? LegalAddress { get; set; }

    /// <summary>
    /// Получает или задает наименование банка.
    /// </summary>
    [JsonPropertyName("bank")]
    public string? BankName { get; set; }

    /// <summary>
    /// Получает или задает БИК (Банковский идентификационный код).
    /// </summary>
    [JsonPropertyName("bik")]
    public string? Bik { get; set; }

    /// <summary>
    /// Получает или задает Корреспондентский счет (К/Сч).
    /// </summary>
    [JsonPropertyName("ks")]
    public string? CorrespondentAccount { get; set; }

    /// <summary>
    /// Получает или задает Расчетный счет (Р/Сч).
    /// </summary>
    [JsonPropertyName("rs")]
    public string? PaymentAccount { get; set; }
}