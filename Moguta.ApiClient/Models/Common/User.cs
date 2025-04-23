using System.Text.Json;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Common;

/// <summary>
/// Представляет сущность "Пользователь" в MogutaCMS.
/// Основано на документации для методов getUsers и importUsers.
/// </summary>
public class User
{
    /// <summary>
    /// Получает или задает уникальный идентификатор пользователя.
    /// Nullable для возможности создания нового пользователя (ID назначается сервером).
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringToNullableLongConverter))] // Строка -> long?
    public long? Id { get; set; }

    [JsonPropertyName("owner")]
    [JsonConverter(typeof(StringToNullableLongConverter))] // Строка "0" -> long?
    public long? Owner { get; set; } // Может быть ID другого пользователя? Сделаем nullable long

    /// <summary>
    /// Получает или задает email адрес пользователя. Обязательное поле.
    /// Используется как основной идентификатор во многих API вызовах.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает ID роли/группы пользователя (например, 1 для администратора, 2 для зарегистрированного пользователя).
    /// Уточните значения по умолчанию в MogutaCMS.
    /// </summary>
    [JsonPropertyName("role")]
    [JsonConverter(typeof(StringToIntConverter))] // Строка -> int
    public int Role { get; set; } = 2; // По умолчанию - зарегистрированный пользователь?

    /// <summary>
    /// Получает или задает полное имя пользователя (или только имя).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Получает или задает фамилию пользователя. Согласно документации, часто не используется.
    /// </summary>
    [JsonPropertyName("sname")]
    public string? SName { get; set; }

    /// <summary>
    /// Получает или задает основной адрес пользователя (вероятно, для доставки/оплаты).
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Получает или задает номер телефона пользователя.
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Получает дату и время создания аккаунта пользователя (только для чтения?).
    /// </summary>
    [JsonPropertyName("date_add")]
    [JsonConverter(typeof(CustomDateTimeOffsetConverter))] // Наш конвертер для дат
    public DateTimeOffset? DateAdd { get; set; }

    /// <summary>
    /// Получает или задает значение, указывающее, заблокирован ли аккаунт пользователя (не может войти).
    /// true = заблокирован, false = активен. API использует 1/0.
    /// </summary>
    [JsonPropertyName("blocked")]
    [JsonConverter(typeof(IntToBoolConverter))] // Строка "0" -> bool
    public bool Blocked { get; set; } = false;

    /// <summary>
    /// Получает или задает статус активности пользователя (включен/отключен?).
    /// true = активен, false = неактивен. API использует 1/0.
    /// </summary>
    [JsonPropertyName("activity")]
    [JsonConverter(typeof(IntToBoolConverter))] 
    public bool Activity { get; set; } = true;

    /// <summary>
    /// Получает или задает дату рождения пользователя. Формат может потребовать кастомный конвертер.
    /// Используем DateOnly, если время нерелевантно (.NET 6+).
    /// </summary>
    [JsonPropertyName("birthday")]
    [JsonConverter(typeof(CustomDateOnlyConverter))]
    public DateOnly? Birthday { get; set; }

    // --- Информация о юридическом лице (хранится у пользователя, копируется в заказы) ---
    /// <summary>
    /// Получает или задает ИНН (Идентификационный номер налогоплательщика) для юр. лица.
    /// </summary>
    [JsonPropertyName("inn")]
    public string? Inn { get; set; }

    /// <summary>
    /// Получает или задает КПП (Код причины постановки на учет) для юр. лица.
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
    /// Получает или задает наименование банка для юр. лица.
    /// </summary>
    [JsonPropertyName("bank")]
    public string? BankName { get; set; }

    /// <summary>
    /// Получает или задает БИК (Банковский идентификационный код) для юр. лица.
    /// </summary>
    [JsonPropertyName("bik")]
    public string? Bik { get; set; }

    /// <summary>
    /// Получает или задает Корреспондентский счет (К/Сч) для юр. лица.
    /// </summary>
    [JsonPropertyName("ks")]
    public string? CorrespondentAccount { get; set; }

    /// <summary>
    /// Получает или задает Расчетный счет (Р/Сч) для юр. лица.
    /// </summary>
    [JsonPropertyName("rs")]
    public string? PaymentAccount { get; set; }

    // --- Дополнительные поля, иногда присутствующие (обычно только для чтения) ---
    /// <summary>
    /// Получает хеш пароля? (только для чтения или для записи при создании?).
    /// </summary>
    [JsonPropertyName("pass")]
    public string? Pass { get; set; } // Пароль (хеш)

    /// <summary>
    /// Получает соль пароля? (только для чтения?).
    /// </summary>
    [JsonPropertyName("salt")]
    public string? Salt { get; set; }

    /// <summary>
    /// Получает код подтверждения? Код активации?
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Получает последний IP адрес пользователя (только для чтения).
    /// </summary>
    [JsonPropertyName("last_ip")]
    public string? LastIp { get; set; }

    [JsonPropertyName("ip")]
    public string? IpAddress { get; set; } // Назвал IpAddress для ясности

    [JsonPropertyName("last_updated")]
    [JsonConverter(typeof(CustomDateTimeOffsetConverter))] // Наш конвертер для дат
    public DateTimeOffset? LastUpdated { get; set; }

    /// <summary>
    /// Получает дату и время последнего визита (только для чтения).
    /// </summary>
    [JsonPropertyName("lastvisit")]
    public DateTimeOffset? LastVisit { get; set; }

    /// <summary>
    /// Получает код восстановления пароля?
    /// </summary>
    [JsonPropertyName("restore_code")]
    public string? RestoreCode { get; set; }

    /// <summary>
    /// Получает или задает дополнительные пользовательские поля.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? CustomFields { get; set; }
}