using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `deleteUser`.
/// </summary>
public class DeleteUserRequestParams
{
    /// <summary>
    /// Получает или задает список email адресов пользователей для удаления.
    /// </summary>
    [JsonPropertyName("email")] // Ключ "email" согласно документации
    public List<string> Emails { get; set; } = [];
}