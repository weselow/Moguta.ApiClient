using Xunit;
using Moguta.ApiClient.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Moguta.ApiClient.Tests;

/// <summary>
/// Юнит-тесты для вспомогательного класса <see cref="SignatureHelper"/>.
/// </summary>
public class SignatureHelperTests
{
    // Используем Reflection для вызова приватного статического метода CalculateSignature
    private static string InvokeCalculateSignature(string token, string method, string rawParametersJson, string secretKey)
    {
        var methodInfo = typeof(SignatureHelper).GetMethod(
            "CalculateSignature",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (methodInfo == null)
        {
            throw new InvalidOperationException("Не удалось найти приватный статический метод 'CalculateSignature'.");
        }

        // NullLogger.Instance можно использовать, т.к. нам важно только само значение подписи в этих тестах
        object? result = methodInfo.Invoke(null, new object[] { token, method, rawParametersJson, secretKey, NullLogger.Instance });

        if (result is string signature)
        {
            return signature;
        }

        throw new InvalidOperationException("Метод 'CalculateSignature' не вернул строку.");
    }

    /// <summary>
    /// Проверяет корректность расчета MD5 хеша для различных входных данных.
    /// Ожидаемые хеши должны быть получены из эталонной PHP реализации.
    /// </summary>
    [Theory]
    // --- РЕАЛЬНЫЕ ХЕШИ, ПОЛУЧЕННЫЕ ИЗ PHP ---
    [InlineData("539469cefb534eebde2bcbcb134c8f66", "getProduct", "{\"page\":1,\"count\":2}", "WPWc7cNbvtoXIj1G", "a4aceaee90ab3b89316be20a66dfa4d4")] // Тест 1: Базовый
    [InlineData("539469cefb534eebde2bcbcb134c8f66", "importProduct", "{\"products\":[{\"cat_id\":2,\"title\":\"Product with < & > \\\" Quotes\",\"price\":25.50,\"url\":\"special-prod\",\"code\":\"SP001\",\"count\":5.0,\"activity\":true}]}", "WPWc7cNbvtoXIj1G", "e83f81023246966c9a9190d3ddb54a12")] // Тест 2: Спецсимволы в значении
    [InlineData("539469cefb534eebde2bcbcb134c8f66", "someMethodWithEmptyParams", "{}", "WPWc7cNbvtoXIj1G", "7ad9cdc14bdf49ef1aac018bb632db66")] // Тест 3: Пустые параметры
    [InlineData("539469cefb534eebde2bcbcb134c8f66", "test", "{\"special\":\"<&>\\\"\",\"cyrillic\":\"тест строки\",\"number\":456}", "WPWc7cNbvtoXIj1G", "ca6e9c5baea8ccf67a1b637176d66f9b")] // Тест 4: Спецсимволы и кириллица
    public void CalculateSignature_Возвращает_Правильный_Хеш(string token, string method, string paramsJson, string secretKey, string expectedHash)
    {
        // Arrange

        // Act
        string actualHash = InvokeCalculateSignature(token, method, paramsJson, secretKey);

        // Assert
        Assert.Equal(expectedHash, actualHash, ignoreCase: true);
    }

    /// <summary>
    /// Проверяет, что валидация проходит успешно при совпадении подписей.
    /// </summary>
    [Fact]
    public void ValidateApiResponseSignature_Возвращает_True_Для_Валидной_Подписи()
    {
        // Arrange
        string token = "validToken";
        string method = "someMethod";
        string paramsJson = "{\"param\":1}";
        string secretKey = "validKey";
        string expectedSignature = InvokeCalculateSignature(token, method, paramsJson, secretKey);

        // Act
        bool isValid = SignatureHelper.ValidateApiResponseSignature(
            expectedSignature, token, method, paramsJson, secretKey, NullLogger.Instance);

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Проверяет, что валидация не проходит при несовпадении подписей.
    /// </summary>
    [Fact]
    public void ValidateApiResponseSignature_Возвращает_False_Для_Невалидной_Подписи()
    {
        // Arrange
        string token = "validToken";
        string method = "someMethod";
        string paramsJson = "{\"param\":1}";
        string secretKey = "validKey";
        string expectedSignature = "очевидно_неверная_подпись";

        // Act
        bool isValid = SignatureHelper.ValidateApiResponseSignature(
            expectedSignature, token, method, paramsJson, secretKey, NullLogger.Instance);

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Проверяет, что валидация считается успешной (с предупреждением в логе), если подпись отсутствует в ответе.
    /// </summary>
    [Fact]
    public void ValidateApiResponseSignature_Возвращает_True_При_Отсутствии_Подписи()
    {
        // Arrange
        string token = "validToken";
        string method = "someMethod";
        string paramsJson = "{\"param\":1}";
        string secretKey = "validKey";
        string? expectedSignature = null;

        // Act
        bool isValid = SignatureHelper.ValidateApiResponseSignature(
            expectedSignature, token, method, paramsJson, secretKey, NullLogger.Instance);

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Проверяет, что валидация не проходит, если отсутствуют необходимые учетные данные (токен или ключ).
    /// </summary>
    [Fact]
    public void ValidateApiResponseSignature_Возвращает_False_При_Отсутствии_Учетных_Данных()
    {
        // Arrange
        string token = "";
        string method = "someMethod";
        string paramsJson = "{\"param\":1}";
        string secretKey = "validKey";
        string expectedSignature = "не_имеет_значения";

        // Act
        bool isValid = SignatureHelper.ValidateApiResponseSignature(
            expectedSignature, token, method, paramsJson, secretKey, NullLogger.Instance);

        // Assert
        Assert.False(isValid);

        // Arrange - Пустой ключ
        token = "validToken";
        secretKey = "";

        // Act
        isValid = SignatureHelper.ValidateApiResponseSignature(
            expectedSignature, token, method, paramsJson, secretKey, NullLogger.Instance);

        // Assert
        Assert.False(isValid);
    }
}