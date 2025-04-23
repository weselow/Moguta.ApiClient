namespace Moguta.ApiClient.Exceptions;

/// <summary>
/// Представляет ошибку во время проверки подписи ответа API.
/// Указывает на потенциальную проблему безопасности или несоответствие конфигурации.
/// Проверяйте свойства <see cref="ExpectedSignature"/> и <see cref="CalculatedSignature"/> для получения деталей.
/// </summary>
public class MogutaApiSignatureException : MogutaApiException
{
    /// <summary>
    /// Получает подпись ('sign'), которая была получена в ответе API.
    /// </summary>
    public string? ExpectedSignature { get; }

    /// <summary>
    /// Получает подпись, которая была рассчитана клиентом на основе данных запроса/ответа.
    /// </summary>
    /// <remarks>В текущей реализации здесь может быть плейсхолдер "[Calculated]", т.к. точное значение не передается в конструктор.</remarks>
    public string? CalculatedSignature { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MogutaApiSignatureException"/>.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="expectedSignature">Подпись, полученная от API.</param>
    /// <param name="calculatedSignature">Подпись, рассчитанная клиентом (может быть плейсхолдером).</param>
    /// <param name="apiMethod">Имя вызванного API метода.</param>
    public MogutaApiSignatureException(string message, string? expectedSignature = null, string? calculatedSignature = null, string? apiMethod = null)
        : base(message, apiMethod) // Передаем сообщение и метод базовому классу
    {
        ExpectedSignature = expectedSignature;
        CalculatedSignature = calculatedSignature;
    }
}