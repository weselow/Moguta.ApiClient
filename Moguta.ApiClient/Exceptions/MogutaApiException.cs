namespace Moguta.ApiClient.Exceptions;

/// <summary>
/// Представляет ошибки, сообщаемые самим Moguta API (например, неверный токен, некорректные параметры).
/// Проверяйте свойства <see cref="ApiErrorCode"/> и <see cref="ApiErrorMessage"/> для получения деталей от API.
/// </summary>
public class MogutaApiException : Exception
{
    /// <summary>
    /// Получает имя API метода, который вызвал ошибку (если известно).
    /// </summary>
    public string? ApiMethod { get; }

    /// <summary>
    /// Получает код ошибки, возвращенный API (если доступен).
    /// Коды ошибок Moguta: 1 - Неверный токен, 2 - Ошибка вызова метода, 3 - API не настроено.
    /// Могут быть и другие коды в зависимости от конкретного метода.
    /// </summary>
    public string? ApiErrorCode { get; }

    /// <summary>
    /// Получает необработанное сообщение об ошибке, возвращенное API (если доступно).
    /// Может содержаться в поле 'message' или 'response' ответа API.
    /// </summary>
    public string? ApiErrorMessage { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MogutaApiException"/>.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="apiMethod">Имя вызванного API метода.</param>
    /// <param name="apiErrorCode">Код ошибки от API.</param>
    /// <param name="apiErrorMessage">Сообщение об ошибке от API.</param>
    /// <param name="innerException">Внутреннее исключение.</param>
    public MogutaApiException(string message, string? apiMethod = null, string? apiErrorCode = null, string? apiErrorMessage = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ApiMethod = apiMethod;
        ApiErrorCode = apiErrorCode;
        // Используем базовое сообщение, если специфичное сообщение от API отсутствует
        ApiErrorMessage = apiErrorMessage ?? (string.IsNullOrWhiteSpace(message) ? base.Message : message);
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MogutaApiException"/>.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="innerException">Внутреннее исключение.</param>
    public MogutaApiException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MogutaApiException"/>.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    public MogutaApiException(string message) : base(message) { }
}