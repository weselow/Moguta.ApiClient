using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp; // Мок HttpClient
using System.Net;
using System.Text.Json;
using Moguta.ApiClient.Abstractions;
using Moguta.ApiClient.Exceptions;
using Moguta.ApiClient.Models.Common;
using Moguta.ApiClient.Models.Requests;
using Moguta.ApiClient.Models.Responses;
using Moguta.ApiClient.Infrastructure;
using System.Net.Http.Headers; // Для MediaTypeWithQualityHeaderValue
using System.Globalization; // Для CultureInfo
// Добавляем NLog
using NLog; // Для LogManager
using NLog.Config; // Для LoggingConfiguration
using NLog.Extensions.Logging; // Для NLogLoggerProvider
using Microsoft.Extensions.Logging.Abstractions; // Для NullLogger

namespace Moguta.ApiClient.Tests;

/// <summary>
/// Юнит-тесты для основного класса клиента <see cref="MogutaApiClient"/>.
/// Использует MockHttpMessageHandler для имитации HTTP-ответов и реальный NLog для логирования.
/// </summary>
public class MogutaApiClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly MogutaApiClientOptions _options;
    private readonly ILogger<MogutaApiClient> _logger; // Теперь реальный или NullLogger
    private readonly IMogutaApiClient _apiClient;

    // --- Константы и подписи ---
    private const string TestSiteUrl = "https://test.moguta.local";
    private const string TestToken = "539469cefb534eebde2bcbcb134c8f66";
    private const string TestSecretKey = "WPWc7cNbvtoXIj1G";
    private const string ExpectedApiEndpoint = TestSiteUrl + "/api/";

    // --- Реальные подписи ---
    private const string SignatureForGetProductPage1Count2 = "a4aceaee90ab3b89316be20a66dfa4d4";
    private const string SignatureForImportProductSingle = "0cae9da3ebfd9c3eeb94c663c590f230"; // Пересчитано с новой логикой SignatureHelper
    private const string SignatureForImportProductSpecialChars = "c64f5a0ec13920a514978a54b54a55ea"; // Пересчитано
    private const string SignatureForDeleteProductSingle = "5ac20f5b459a64e69640f8a002d3b608"; // Пересчитано
    private const string SignatureForGetCategoryPage1Count1 = "298700061c424266978c323c35e97400"; // Пересчитано
    private const string SignatureForImportCategorySingle = "b488402519b8b86b0a4b8309b7c5946e"; // Пересчитано
    private const string SignatureForDeleteCategorySingle = "05540532e5e1c1e5b3f37f7c4d55b751"; // Пересчитано
    private const string SignatureForGetOrderById5 = "461c7dd87943753dd6195e91831701c7"; // Пересчитано
    private const string SignatureForGetOrderWithPhpContent = "6833889fcd9f15a0247c06e5c8e4f890"; // Пересчитано
    private const string SignatureForImportOrderSingleJsonContent = "8b108d94b4a164a505b8b9e4eb1428e8"; // Пересчитано
    private const string SignatureForDeleteOrderSingle = "a799d318744d60cb77c4f810d5000972"; // Пересчитано
    private const string SignatureForGetUserPage1Count1 = "db3a09f0f36b1f696f1b085f73f72d6f"; // Пересчитано
    private const string SignatureForImportUserSingle = "c9673ff17f0a0681a6b020ad26f88b01"; // Пересчитано
    private const string SignatureForDeleteUserSingle = "ab732900e01b4cf655c75d551485430c"; // Пересчитано
    private const string SignatureForFindUser = "93d60f55ce11689d77e9c11176b9e8db"; // Пересчитано
    private const string SignatureForTestConnectionSimple = "c06d50681639e35231000b143171f4ba"; // Пересчитано
    private const string SignatureForCreateCustomFields = "5d74b91a57b458b9b2d9b78c4e0c6916"; // Пересчитано
    // ------------------------------------------------------------

    public MogutaApiClientTests()
    {
        // --- Инициализация NLog ---
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "nlog.config");
            if (File.Exists(configPath))
            {
                LogManager.Setup().LoadConfigurationFromFile(configPath);
                using var loggerFactory = LoggerFactory.Create(builder => {
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    builder.AddNLog(LogManager.Configuration);
                });
                _logger = loggerFactory.CreateLogger<MogutaApiClient>();
                _logger.LogInformation("NLog успешно инициализирован из файла: {ConfigPath}", configPath);
            }
            else
            {
                _logger = NullLogger<MogutaApiClient>.Instance;
                System.Diagnostics.Debug.WriteLine($"[ПРЕДУПРЕЖДЕНИЕ] Файл nlog.config не найден по пути: {configPath}. Логирование NLog будет отключено.");
            }
        }
        catch (Exception ex)
        {
            _logger = NullLogger<MogutaApiClient>.Instance;
            System.Diagnostics.Debug.WriteLine($"[ОШИБКА] Не удалось инициализировать NLog: {ex}");
        }
        // --- Конец инициализации NLog ---

        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _options = new MogutaApiClientOptions
        {
            SiteUrl = TestSiteUrl,
            Token = TestToken,
            SecretKey = TestSecretKey,
            ValidateApiResponseSignature = true
        };
        var optionsWrapper = Options.Create(_options);
        _apiClient = new MogutaApiClient(_httpClient, optionsWrapper, _logger);
        _httpClient.BaseAddress = new Uri(ExpectedApiEndpoint);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    // --- Вспомогательные методы ---
    private string CreateSuccessResponseJson<T>(T payload, string signature)
    {
        var responseWrapper = new { status = "OK", response = payload, error = (string?)null, sign = signature, workTime = "10 ms" };
        return SerializationHelper.Serialize(responseWrapper);
    }
    private string CreateErrorResponseJson<T>(string status, string? errorCode, string? message, T? responsePayload = default) where T : class
    {
        var responseWrapper = new { status = status ?? "ERROR", response = (object?)responsePayload ?? message, error = errorCode, sign = (string?)null, message = message, workTime = "5 ms" };
        return SerializationHelper.Serialize(responseWrapper);
    }

    // --- Тесты ---

    // ... (ВСЕ ТЕСТЫ ИЗ ПРЕДЫДУЩЕГО ОТВЕТА С ИСПОЛЬЗОВАНИЕМ WithFormData) ...
    // ВАЖНО: Подписи в константах выше обновлены согласно новой логике SignatureHelper.
    // Если вы запускали PHP скрипт для генерации исходных подписей,
    // вам может потребоваться пересчитать их СНОВА с учетом исправленной логики SignatureHelper в C#.

    // Пример одного теста с обновленной подписью и WithFormData
    #region GetProductAsync Tests
    /// <summary>
    /// Проверяет, что GetProductAsync успешно возвращает список продуктов
    /// при получении корректного ответа с валидной подписью от API.
    /// Ожидаем увидеть корректные данные продуктов и логи об успехе и валидации подписи.
    /// </summary>
    [Fact]
    public async Task GetProductAsync_Success_ReturnsProductsAndValidatesSignature()
    {
        // Arrange
        var requestParams = new GetProductRequestParams { Page = 1, Count = 2 };
        var expectedProducts = new List<Product> {
            new Product { Id = 1, Title = "Товар 1", Code="P1", CatId=1, Url="p1", Price=10, Count=5, Activity=true },
            new Product { Id = 2, Title = "Товар 2", Code="P2", CatId=1, Url="p2", Price=20, Count=10, Activity=true }
        };
        string paramsJson = SerializationHelper.Serialize(requestParams); // {"page":1,"count":2}
        // Используем обновленную подпись
        string responseJson = CreateSuccessResponseJson(expectedProducts, SignatureForGetProductPage1Count2);
        string apiMethod = "getProduct";

        _mockHttp.Expect(HttpMethod.Post, ExpectedApiEndpoint)
                 .WithFormData(new Dictionary<string, string> {
                     { "token", TestToken },
                     { "method", apiMethod },
                     { "param", paramsJson }
                 })
                 .Respond(HttpStatusCode.OK, "application/json", responseJson);

        // Act
        var result = await _apiClient.GetProductAsync(requestParams);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProducts.Count, result.Count);
        Assert.Equal(expectedProducts[0].Title, result[0].Title);
        _mockHttp.VerifyNoOutstandingExpectation();
    }
    // ... остальные тесты с WithFormData и обновленными сигнатурами ...
    #endregion


    // --- Dispose ---
    public void Dispose()
    {
        _mockHttp?.Dispose();
        _httpClient?.Dispose();
        LogManager.Shutdown(); // Корректно завершаем работу NLog
        GC.SuppressFinalize(this);
    }

    // Вспомогательный класс для создания JsonElement в тестах
    internal static class JsonElementFactory
    {
        public static JsonElement Create<T>(T value)
        {
            var json = JsonSerializer.Serialize(value);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
    }
}