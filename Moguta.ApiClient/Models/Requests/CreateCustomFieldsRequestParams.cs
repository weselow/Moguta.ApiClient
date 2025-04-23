using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `createCustomFields`.
/// </summary>
public class CreateCustomFieldsRequestParams
{
    /// <summary>
    /// Получает или задает список определений дополнительных полей для создания или обновления.
    /// </summary>
    [JsonPropertyName("data")]
    public List<CustomFieldDefinition> Data { get; set; } = [];
}