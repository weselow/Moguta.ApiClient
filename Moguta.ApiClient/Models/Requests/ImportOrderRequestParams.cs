using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

/// <summary>
/// Параметры для API метода `importOrder`.
/// </summary>
public class ImportOrderRequestParams
{
    /// <summary>
    /// Получает или задает список заказов для импорта (создания или обновления).
    /// Рекомендуемый размер пакета - до 100 записей.
    /// </summary>
    /// <remarks>
    /// **Важно:** Для передачи позиций заказа используйте свойство <c>OrderItems</c> в объектах <see cref="MogutaOrder"/>.
    /// Клиент автоматически сериализует <c>OrderItems</c> в JSON и отправит в поле 'order_content'.
    /// Убедитесь, что API сервера настроен на прием JSON в этом поле.
    /// </remarks>
    [JsonPropertyName("orders")]
    public List<MogutaOrder> Orders { get; set; } = [];

    // Флаг 'enableUpdate' не показан в примерах для заказов,
    // обновление, вероятно, происходит неявно при наличии ID.
}