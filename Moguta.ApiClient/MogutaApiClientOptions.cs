using System.ComponentModel.DataAnnotations;
using System.Reflection;

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
    public bool ValidateApiResponseSignature { get; set; } = false;

    /// <summary>
    /// Получает или задает необязательный таймаут для API запросов, выполняемых HttpClient.
    /// Если не задано, будет использоваться таймаут по умолчанию для HttpClient (обычно 100 секунд).
    /// </summary>
    /// <value>Длительность таймаута запроса или <c>null</c> для использования значения по умолчанию.</value>
    public TimeSpan? RequestTimeout { get; set; }

    /// <summary>
    /// Количество заказов на страницу (максимум 100)
    /// </summary>
    public int MaxGetProductPerPageCount { get; set; } = 100;

    /// <summary>
    /// Максимальное количество товаров в одном запросе к API.
    /// </summary>
    public int MaxImportProductBachSize { get; set; } = 100;
   

    /// <summary>
    /// Количество пользователей на страницу (максимум 250)
    /// </summary>
    public int MaxGetUsersPerPageCount { get; set; } = 250;
    public int MaxImportUsersBatchSize { get; set; } = 100;

    /// <summary>
    ///  Количество категорий на страницу (максимум 250)
    /// </summary>
    public int MaxGetCategoryPerPageCount { get; set; } = 250;
    public int MaxImportCategoryBatchSize { get; set; } = 100;

    /// <summary>
    /// Количество заказов на страницу (максимум 250)
    /// </summary>
    public int MaxGetOrderPerPageCount { get; set; } = 250;
    public int MaxImportOrderBatchSize { get; set; } = 100;


    public void UpdateFrom<T>(T other) where T : MogutaApiClientOptions
    {
        foreach (PropertyInfo prop in this.GetType().GetProperties())
        {
            if (!prop.CanWrite) continue; // Проверяем, можно ли записывать в свойство
            var value = prop.GetValue(other);
            prop.SetValue(this, value);
        }
    }
}