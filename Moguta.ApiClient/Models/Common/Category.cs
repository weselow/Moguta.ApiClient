using Moguta.ApiClient.Infrastructure.Converters; // Для IntToBoolConverter
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Common;

/// <summary>
/// Представляет сущность "Категория" в MogutaCMS.
/// Основано на документации и примерах импорта/экспорта.
/// </summary>
public class Category
{
    /// <summary>
    /// Получает или задает уникальный идентификатор категории.
    /// Nullable для возможности создания новой категории (ID назначается сервером).
    /// </summary>
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    /// <summary>
    /// Получает или задает название категории. Обязательное поле.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает URL-псевдоним (slug) для категории (например, "electronics").
    /// Обязательное поле (или генерируется автоматически сервером, если пустое?).
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает ID родительской категории. 0 для корневых категорий.
    /// </summary>
    [JsonPropertyName("parent")]
    public long Parent { get; set; } = 0;

    /// <summary>
    /// Получает полный путь URL родительских категорий (только для чтения, предоставляется API при GET-запросе).
    /// Пример: "catalog/electronics"
    /// </summary>
    [JsonPropertyName("parent_url")]
    public string? ParentUrl { get; set; } // Только чтение

    /// <summary>
    /// Получает или задает порядковый номер для сортировки.
    /// </summary>
    [JsonPropertyName("sort")]
    public int Sort { get; set; } = 0;

    /// <summary>
    /// Получает или задает HTML-содержимое/описание для страницы категории.
    /// </summary>
    [JsonPropertyName("html_content")]
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Получает или задает SEO Meta Title для страницы категории.
    /// </summary>
    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Получает или задает SEO Meta Keywords для страницы категории.
    /// </summary>
    [JsonPropertyName("meta_keywords")]
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Получает или задает SEO Meta Description для страницы категории.
    /// </summary>
    [JsonPropertyName("meta_desc")]
    public string? MetaDesc { get; set; }

    /// <summary>
    /// Получает или задает значение, указывающее, должна ли категория быть скрытой.
    /// true = скрыть (невидима), false = видима. API использует 1/0.
    /// </summary>
    [JsonPropertyName("invisible")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool Invisible { get; set; } = false; // По умолчанию видима

    /// <summary>
    /// Получает или задает идентификатор, используемый для синхронизации с 1С.
    /// Имя свойства начинается с цифры, что допустимо в C#, но может выглядеть непривычно.
    /// </summary>
    [JsonPropertyName("1c_id")]
    public string? ExternalId1C { get; set; }

    /// <summary>
    /// Получает или задает URL или имя файла изображения категории.
    /// </summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Получает или задает CSS класс иконки или URL для пункта меню категории.
    /// </summary>
    [JsonPropertyName("menu_icon")]
    public string? MenuIcon { get; set; }

    /// <summary>
    /// Получает или задает наценку (процент или абсолютное значение?), применяемую к товарам в этой категории.
    /// Требует уточнения типа и единиц измерения.
    /// </summary>
    [JsonPropertyName("rate")]
    [JsonConverter(typeof(StringToDecimalConverter))] // Используем конвертер, т.к. тип неизвестен
    public decimal Rate { get; set; } = 0; // По умолчанию 0

    /// <summary>
    /// Получает или задает единицу измерения по умолчанию для товаров в этой категории (если применимо).
    /// Например, "шт.", "кг".
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    /// <summary>
    /// Получает или задает флаг, указывающий, следует ли включать категорию в экспорт (например, YML).
    /// true = да, false = нет. API использует 1/0.
    /// </summary>
    [JsonPropertyName("export")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool Export { get; set; } = true; // По умолчанию экспортируема

    /// <summary>
    /// Получает или задает дополнительный SEO контент/текстовый блок для страницы категории.
    /// </summary>
    [JsonPropertyName("seo_content")]
    public string? SeoContent { get; set; }

    /// <summary>
    /// Получает или задает статус активности (включена/отключена).
    /// true = активна, false = неактивна. API использует 1/0.
    /// </summary>
    [JsonPropertyName("activity")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool Activity { get; set; } = true; // По умолчанию активна

    // Дополнительные поля, часто возвращаемые GET-запросами (только для чтения)
    /// <summary>
    /// Получает уровень вложенности категории (только для чтения).
    /// </summary>
    [JsonPropertyName("level")]
    [JsonConverter(typeof(StringToNullableIntConverter))]
    public int? Level { get; set; }

    /// <summary>
    /// Получает имя файла изображения (часть ImageUrl) (только для чтения).
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// Получает список изображений, если API поддерживает несколько (только для чтения).
    /// </summary>
    [JsonPropertyName("images")]
    public List<string>? Images { get; set; }
}