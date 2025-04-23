using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Common;

/// <summary>
/// Определяет структуру дополнительного поля, создаваемого для заказов через API метод 'createCustomFields'.
/// </summary>
public class CustomFieldDefinition
{
    /// <summary>
    /// Получает или задает имя/метку дополнительного поля. Обязательное поле.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает тип дополнительного поля. Обязательное поле.
    /// Поддерживаемые типы (из примера): "input", "select", "checkbox", "radiobutton", "textarea".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // например, "input", "select"

    /// <summary>
    /// Получает или задает список возможных значений/опций для типов "select" или "radiobutton".
    /// </summary>
    [JsonPropertyName("variants")]
    public List<string>? Variants { get; set; } // Применимо только для select/radiobutton

    /// <summary>
    /// Получает или задает значение, указывающее, является ли поле обязательным при оформлении заказа.
    /// true = обязательно, false = необязательно. API использует 1/0.
    /// </summary>
    [JsonPropertyName("required")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool Required { get; set; } = false;

    /// <summary>
    /// Получает или задает значение, указывающее, активно ли поле (включено).
    /// true = активно, false = неактивно. API использует 1/0.
    /// </summary>
    [JsonPropertyName("active")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool Active { get; set; } = true;

    // Примечание: API пример не показывает поле 'id' при отправке или в ответе для этой операции.
    // Обновление, вероятно, происходит по совпадению поля 'name'.
}