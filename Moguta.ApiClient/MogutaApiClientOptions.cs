using System.ComponentModel.DataAnnotations;

namespace Moguta.ApiClient;

/// <summary>
/// Опции конфигурации для <see cref="MogutaApiClient"/>.
/// Определяет параметры подключения и настройки поведения клиента.
/// </summary>
public class MogutaApiClientOptions
{
    /// <summary>
    /// Получает или задает базовый URL сайта MogutaCMS, где расположено API.
    /// Должен быть корневым URL сайта (например, "https://your-moguta-site.ru").
    /// Клиент автоматически добавит путь "/api".
    /// </summary>
    /// <remarks>
    /// Пример: "https://domain.name"
    /// </remarks>
    [Required(ErrorMessage = "Требуется указать SiteUrl.")]
    [Url(ErrorMessage = "SiteUrl должен быть валидным абсолютным URL.")]
    public string SiteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает API Токен, сгенерированный в панели администратора MogutaCMS (Настройки -> API).
    /// Используется для аутентификации запросов.
    /// </summary>
    [Required(ErrorMessage = "Требуется указать Token.")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает Секретный ключ, определенный в панели администратора MogutaCMS (Настройки -> API).
    /// Используется клиентом для проверки подписи ответа ('sign'), полученного от сервера.
    /// </summary>
    [Required(ErrorMessage = "Требуется указать SecretKey.")]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задает значение, указывающее, следует ли проверять подпись ('sign' поле)
    /// ответа, полученного от API сервера Moguta.
    /// Значение по умолчанию: <c>true</c>. Отключение проверки крайне не рекомендуется из соображений безопасности.
    /// </summary>
    /// <value><c>true</c> для проверки подписей; иначе <c>false</c>.</value>
    public bool ValidateApiResponseSignature { get; set; } = true;

    /// <summary>
    /// Получает или задает необязательный таймаут для API запросов, выполняемых HttpClient.
    /// Если не задано, будет использоваться таймаут по умолчанию для HttpClient (обычно 100 секунд).
    /// </summary>
    /// <value>Длительность таймаута запроса или <c>null</c> для использования значения по умолчанию.</value>
    public TimeSpan? RequestTimeout { get; set; }
}