using Microsoft.Extensions.Logging; // Для ILogger
using System.Security.Cryptography;
using System.Text;
// System.Web нужен для HttpUtility.HtmlEncode.
// Если не используется ASP.NET Core, его нужно заменить ручной заменой символов.
// Для проектов ASP.NET Core добавьте <FrameworkReference Include="Microsoft.AspNetCore.App" /> в .csproj
using System.Web;

namespace Moguta.ApiClient.Infrastructure;

/// <summary>
/// Вспомогательный класс для расчета и проверки подписи ответа Moguta API.
/// </summary>
internal static class SignatureHelper
{
    /// <summary>
    /// Проверяет подпись, полученную в ответе от Moguta API.
    /// </summary>
    /// <param name="expectedSignature">Значение поля 'sign' из ответа API.</param>
    /// <param name="token">Ваш API токен.</param>
    /// <param name="method">Имя вызванного API метода.</param>
    /// <param name="rawParametersJson">Исходная JSON строка параметров, *отправленная* в запросе ('param').</param>
    /// <param name="secretKey">Ваш секретный ключ.</param>
    /// <param name="logger">Экземпляр логгера.</param>
    /// <returns><c>true</c>, если подпись верна или отсутствует (с предупреждением); <c>false</c> при неверной подписи или отсутствии учетных данных.</returns>
    public static bool ValidateApiResponseSignature(
        string? expectedSignature,
        string token,
        string method,
        string rawParametersJson,
        string secretKey,
        ILogger logger)
    {
        // Если подпись не пришла, считаем валидным (но логируем предупреждение)
        if (string.IsNullOrEmpty(expectedSignature))
        {
            logger.LogWarning("Ответ API не содержит подпись ('sign' поле). Проверка пропускается.");
            return true; // Поведение по умолчанию - разрешать ответы без подписи
        }

        // Не можем проверить подпись без токена или ключа
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(secretKey))
        {
            logger.LogError("Невозможно проверить подпись ответа: Токен или Секретный Ключ отсутствуют в конфигурации.");
            return false;
        }

        // Рассчитываем подпись на основе полученных данных
        string calculatedSignature = CalculateSignature(token, method, rawParametersJson, secretKey, logger);

        // Сравниваем ожидаемую и рассчитанную подписи (без учета регистра)
        bool isValid = string.Equals(expectedSignature, calculatedSignature, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            // Логируем ошибку несовпадения подписей
            // Обрезаем JSON параметров для лога
            string paramsSnippet = rawParametersJson.Length > 100 ? rawParametersJson.Substring(0, 100) + "..." : rawParametersJson;
            logger.LogError("Несовпадение подписи ответа API! Ожидалось: {ExpectedSignature}, Рассчитано: {CalculatedSignature}. " +
                            "Метод: {ApiMethod}, Токен: {Token}, ParamsJson (начало): {ParamsJsonSnippet}, SecretKey: ***скрыто***",
                            expectedSignature, calculatedSignature, method, token, paramsSnippet);
        }
        else
        {
            // Логируем успешную проверку (уровень Debug)
            logger.LogDebug("Подпись ответа API успешно проверена. Подпись: {Signature}", calculatedSignature);
        }

        return isValid;
    }

    /// <summary>
    /// Рассчитывает строку подписи по алгоритму Moguta.
    /// Алгоритм: md5(token + method + processed_param_json + secretKey)
    /// Где processed_param_json = htmlspecialchars(raw_param_json)
    /// (Шаг str_replace('amp;', '', ...) был убран как некорректный/излишний).
    /// </summary>
    /// <param name="token">API Токен.</param>
    /// <param name="method">Имя API метода.</param>
    /// <param name="rawParametersJson">Исходная JSON строка параметров.</param>
    /// <param name="secretKey">Секретный ключ.</param>
    /// <param name="logger">Экземпляр логгера.</param>
    /// <returns>Рассчитанный MD5 хеш в нижнем регистре.</returns>
    private static string CalculateSignature(string token, string method, string rawParametersJson, string secretKey, ILogger logger)
    {
        // Шаг 1: Эквивалент htmlspecialchars (только необходимые замены)
        string htmlSpecialCharsResult = rawParametersJson
            .Replace("&", "&amp;")    // & -> &amp;
            .Replace("<", "&lt;")     // < -> &lt;
            .Replace(">", "&gt;")     // > -> &gt;
            .Replace("\"", "&quot;");  // " -> &quot;

        // Шаг 2: Эквивалент str_replace('amp;', '', ...)
        // Ищем и удаляем буквальную подстроку "amp;"
        string processedParams = htmlSpecialCharsResult.Replace("amp;", ""); // <-- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ

        // Шаг 3: Конкатенация компонентов для хеширования
        string stringToHash = $"{token}{method}{processedParams}{secretKey}";
        logger.LogTrace("Строка для хеширования подписи (htmlspecialchars -> str_replace): {StringToHash}", stringToHash);

        // Шаг 4: Вычисление MD5 хеша
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(stringToHash);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // Шаг 5: Преобразование хеша в строку
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}