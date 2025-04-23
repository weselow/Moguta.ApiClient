using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations; // Для атрибута Required

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `findUser`.
/// </summary>
public class FindUserRequestParams
{
    /// <summary>
    /// Получает или задает email адрес пользователя для поиска. Обязательное поле.
    /// </summary>
    [JsonPropertyName("email")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется указать Email для поиска пользователя.")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email адреса.")]
    public string Email { get; set; } = string.Empty;
}