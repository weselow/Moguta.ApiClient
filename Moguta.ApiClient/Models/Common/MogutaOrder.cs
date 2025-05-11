using System.Text.Json; // Для JsonElement
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Common;

/// <summary>
/// Представляет сущность "Заказ" в MogutaCMS.
/// </summary>
public class MogutaOrder
{
    /// <summary>
    /// Получает или задает уникальный идентификатор заказа.
    /// Nullable для возможности создания нового заказа (ID назначается сервером).
    /// </summary>
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    /// <summary>
    /// Получает или задает дату и время последнего обновления заказа (только для чтения?).
    /// </summary>
    [JsonPropertyName("updata_date")] // Опечатка 'updata' из API
    public DateTimeOffset? UpdateDate { get; set; }

    /// <summary>
    /// Получает или задает дату и время создания заказа (только для чтения?).
    /// </summary>
    [JsonPropertyName("add_date")]
    public DateTimeOffset? AddDate { get; set; }

    /// <summary>
    /// Получает или задает дату и время оплаты заказа. Null, если не оплачен.
    /// </summary>
    [JsonPropertyName("pay_date")]
    public DateTimeOffset? PayDate { get; set; }

    /// <summary>
    /// Получает или задает дату и время закрытия/завершения/отмены заказа.
    /// </summary>
    [JsonPropertyName("close_date")]
    public DateTimeOffset? CloseDate { get; set; }

    /// <summary>
    /// Получает или задает email адрес клиента, оформившего заказ. Вероятно, обязательное поле при создании.
    /// </summary>
    [JsonPropertyName("user_email")]
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает номер телефона клиента.
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Получает или задает адрес доставки одной строкой (может быть устаревшим, если используется address_parts).
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Получает или задает структурированные детали адреса доставки. Рекомендуется использовать при создании новых заказов.
    /// </summary>
    [JsonPropertyName("address_parts")]
    public MogutaOrderAddress? AddressParts { get; set; }

    /// <summary>
    /// Получает или задает общую сумму позиций заказа (без доставки) в валюте заказа.
    /// </summary>
    [JsonPropertyName("summ")]
    [JsonConverter(typeof(StringToDecimalConverter))]
    public decimal Sum { get; set; }

    /// <summary>
    /// [Только для чтения] Получает необработанную PHP сериализованную строку, представляющую позиции заказа.
    /// Заполняется при ПОЛУЧЕНИИ заказов через API. Автоматическая десериализация не поддерживается.
    /// </summary>
    /// <remarks>
    /// Используйте свойство <see cref="OrderItems"/> для работы с позициями заказа в C#.
    /// </remarks>
    [JsonIgnore]
    public string? OrderContent { get; set; }

    /// <summary>
    /// [Для записи] Получает или задает список позиций заказа. Используется при ИМПОРТЕ заказов.
    /// Этот список будет сериализован в JSON и отправлен в поле 'order_content'.
    /// Требует, чтобы API сервера мог обработать JSON в этом поле.
    /// </summary>
    [JsonPropertyName("order_content")]  // Игнорировать при стандартной сериализации/десериализации самого Order
    public List<MogutaOrderItem>? OrderItems { get; set; }

    /// <summary>
    /// Получает или задает ID выбранного способа доставки.
    /// </summary>
    [JsonPropertyName("delivery_id")]
    public long? DeliveryId { get; set; }

    /// <summary>
    /// Получает или задает стоимость доставки в валюте заказа.
    /// </summary>
    [JsonPropertyName("delivery_cost")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))]
    public decimal? DeliveryCost { get; set; }

    /// <summary>
    /// Получает или задает дополнительные опции или детали, связанные с доставкой (например, ID пункта выдачи, трек-номер).
    /// Структура может варьироваться. Обрабатывать как строку или объект? Пример показывает null.
    /// </summary>
    [JsonPropertyName("delivery_options")]
    public object? DeliveryOptions { get; set; } // Использовать object или string, десериализовать вручную при необходимости

    /// <summary>
    /// Получает или задает ID выбранного способа оплаты.
    /// </summary>
    [JsonPropertyName("payment_id")]
    public long? PaymentId { get; set; }

    /// <summary>
    /// Получает статус оплаты (только для чтения?). 1 = оплачен, 0 = не оплачен.
    /// Используйте <see cref="PayDate"/> или <see cref="StatusId"/> для более надежного определения статуса оплаты.
    /// </summary>
    [JsonPropertyName("paided")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool? Paided { get; set; }

    /// <summary>
    /// Получает или задает ID статуса заказа. Обязательно для обновлений, опционально при создании (по умолчанию 0?).
    /// 0=Новый, 1=Ожидает оплаты, 2=Оплачен, 3=В доставке, 4=Отменен, 5=Выполнен, 6=В обработке и т.д.
    /// </summary>
    [JsonPropertyName("status_id")]
    public int StatusId { get; set; } = 0; // По умолчанию 'Новый'? Уточнить в Moguta.

    /// <summary>
    /// Получает или задает комментарий, оставленный клиентом при оформлении заказа.
    /// </summary>
    [JsonPropertyName("user_comment")]
    public string? UserComment { get; set; }

    /// <summary>
    /// Получает или задает внутренний комментарий, добавленный менеджером магазина.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Получает или задает информацию о юридическом лице, предоставленную клиентом.
    /// </summary>
    [JsonPropertyName("yur_info")]
    public MogutaOrderYurInfo? YurInfo { get; set; }

    /// <summary>
    /// Получает или задает имя покупателя (может отличаться от имени в аккаунте пользователя).
    /// </summary>
    [JsonPropertyName("name_buyer")]
    public string? NameBuyer { get; set; }

    /// <summary>
    /// Получает или задает запрошенную дату доставки. Формат из примера: "dd.MM.yyyy"?
    /// Хранится как строка, требует ручного парсинга или кастомного конвертера.
    /// </summary>
    [JsonPropertyName("date_delivery")]
    // [JsonConverter(typeof(RuDateConverter))] // Подключить при необходимости
    public string? DateDelivery { get; set; }

    /// <summary>
    /// Получает или задает запрошенный интервал времени доставки (например, "10:00-14:00").
    /// Присутствует в примере ответа API.
    /// </summary>
    [JsonPropertyName("delivery_interval")]
    public string? DeliveryInterval { get; set; }

    /// <summary>
    /// Получает или задает IP адрес, с которого был оформлен заказ.
    /// </summary>
    [JsonPropertyName("ip")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Получает публичный номер заказа (например, "M-0106655179300"). Обычно генерируется сервером (только для чтения?).
    /// </summary>
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    /// <summary>
    /// Получает хеш заказа для гостевого доступа? (Пример показывает пустую строку). (только для чтения?).
    /// </summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    /// <summary>
    /// Получает временную метку последней выгрузки в 1С (только для чтения).
    /// </summary>
    [JsonPropertyName("1c_last_export")]
    public DateTimeOffset? ExternalSyncDate1C { get; set; } // Только чтение

    /// <summary>
    /// Получает или задает ID или имя склада, связанного с заказом.
    /// </summary>
    [JsonPropertyName("storage")]
    public string? Storage { get; set; }

    /// <summary>
    /// Получает общую сумму позиций заказа (без доставки) в валюте магазина по умолчанию.
    /// </summary>
    [JsonPropertyName("summ_shop_curr")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Используем Nullable конвертер
    public decimal? SumShopCurrency { get; set; }

    /// <summary>
    /// Получает стоимость доставки в валюте магазина по умолчанию.
    /// </summary>
    [JsonPropertyName("delivery_shop_curr")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Используем Nullable конвертер
    public decimal? DeliveryShopCurrency { get; set; }

    /// <summary>
    /// Получает ISO код валюты, используемой для полей 'summ' и 'delivery_cost' (например, "RUR").
    /// </summary>
    [JsonPropertyName("currency_iso")]
    public string? CurrencyIso { get; set; }

    /// <summary>
    /// Получает или задает дополнительные пользовательские поля, связанные с заказом.
    /// Структура зависит от использования метода `createCustomFields`.
    /// Используется словарь для гибкости. Ключи - имена/ID полей, значения - отправленные данные.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? CustomFields { get; set; }
}