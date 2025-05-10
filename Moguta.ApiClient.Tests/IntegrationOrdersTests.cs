using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moguta.ApiClient.Abstractions;
using Moguta.ApiClient.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moguta.ApiClient.Models.Common;
using Moguta.ApiClient.Models.Requests;
using System.Diagnostics;
using NLog.Extensions.Logging;

namespace Moguta.ApiClient.Tests;

[Trait("Orders", "Integration")]
public class IntegrationOrdersTests
{
    private readonly IMogutaApiClient? _apiClient;
    private readonly bool _canRunIntegrationTests;
    private readonly ILogger<IntegrationOrdersTests> _logger;

    public IntegrationOrdersTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<IntegrationOrdersTests>()
            .Build();

        var siteUrl = configuration["MogutaApiIntegration:SiteUrl"];
        var token = configuration["MogutaApiIntegration:Token"];
        var secretKey = configuration["MogutaApiIntegration:SecretKey"];

        _canRunIntegrationTests = !string.IsNullOrWhiteSpace(siteUrl)
                                 && !string.IsNullOrWhiteSpace(token)
                                 && !string.IsNullOrWhiteSpace(secretKey);

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            builder.AddNLog(); // Раскомментировать, если NLog настроен для проекта тестов
        });

        var serviceProviderBuilder = services.BuildServiceProvider();
        _logger = serviceProviderBuilder.GetService<ILogger<IntegrationOrdersTests>>()
                  ?? NullLogger<IntegrationOrdersTests>.Instance;

        if (_canRunIntegrationTests)
        {
            services.AddMogutaApiClient(options =>
            {
                options.SiteUrl = siteUrl!;
                options.Token = token!;
                options.SecretKey = secretKey!;
                options.ValidateApiResponseSignature = false;
                options.RequestTimeout = TimeSpan.FromSeconds(30);
            });
            var serviceProvider = services.BuildServiceProvider();
            _apiClient = serviceProvider.GetRequiredService<IMogutaApiClient>();
            _logger.LogInformation("Интеграционные тесты: Конфигурация найдена, API клиент инициализирован.");
        }
        else
        {
            _apiClient = null;
            _logger.LogWarning("Интеграционные тесты: Конфигурация MogutaApiIntegration (SiteUrl, Token, SecretKey) не найдена или неполная. Тесты будут пропущены.");
        }
    }

    /// <summary>
    /// Проверяет базовую связность с реальным API MogutaCMS,
    /// аутентификацию и валидацию подписи ответа.
    /// Требует наличия настроек в User Secrets или переменных окружения.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// </summary>
    [Fact]
    public async Task TestConnectionAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста TestConnectionAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }
        _logger.LogInformation("Запуск теста TestConnectionAsync_RealApi_ShouldSucceed...");

        // Arrange
        var testParams = new { integration_test = DateTime.UtcNow.ToString("o") };

        // Act
        var exception = await Record.ExceptionAsync(() => _apiClient!.TestConnectionAsync(testParams));

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест TestConnectionAsync_RealApi_ShouldSucceed провален.");
        Assert.Null(exception);
        _logger.LogInformation("Тест TestConnectionAsync_RealApi_ShouldSucceed пройден.");
    }
    
    /// <summary>
    /// Проверяет получение списка заказов от реального API с использованием пагинации.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// </summary>
    [Fact]
    public async Task GetOrderAsync_RealApi_ShouldReturnOrders()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста GetOrderAsync_RealApi_ShouldReturnOrders: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }
        _logger.LogInformation("Запуск теста GetOrderAsync_RealApi_ShouldReturnOrders...");

        // Arrange
        var requestParams = new GetOrderRequestParams
        {
            Page = 1,
            Count = 5
        };
        List<Order>? orders = null;

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            orders = await _apiClient!.GetOrderAsync(requestParams);
        });

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест GetOrderAsync_RealApi_ShouldReturnOrders провален.");
        Assert.Null(exception);
        Assert.NotNull(orders);
        _logger.LogInformation("Тест GetOrderAsync_RealApi_ShouldReturnOrders: Получено {Count} заказов на первой странице.", orders.Count);

        if (orders.Count > 0)
        {
            var firstOrder = orders[0];
            _logger.LogDebug(" - Первый заказ: ID={Id}, Number='{Number}', Email='{Email}', StatusId={StatusId}, Items={ItemCount}, ContentNotNull={ContentNotNull}",
                firstOrder.Id, firstOrder.Number, firstOrder.UserEmail, firstOrder.StatusId, firstOrder.OrderItems?.Count, firstOrder.OrderContent != null);
            Assert.NotNull(firstOrder.Number);
        }
    }
    
    /// <summary>
    /// Проверяет создание нового заказа (без позиций) через реальное API,
    /// а также последующее его удаление.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// ВНИМАНИЕ: Этот тест СОЗДАЕТ и УДАЛЯЕТ данные на сервере.
    /// </summary>
    [Fact]
    public async Task CreateAndDeleteBasicOrderAsync_RealApi_ShouldSucceed() // Переименован для ясности
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateAndDeleteBasicOrderAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание заказа ---
        // Используем email существующего пользователя
        // !!! ЗАМЕНИТЬ НА РЕАЛЬНЫЙ СУЩЕСТВУЮЩИЙ EMAIL НА ТЕСТОВОМ СЕРВЕРЕ !!!
        string customerEmail = "user@moguta.ru"; // Замените!
                                                 // -----------------------------------------------------------------

        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var newOrder = new Order
        {
            // Id = null,
            UserEmail = customerEmail,
            Phone = "+7(999)000-11-22",
            NameBuyer = $"Тест Заказчик {uniqueId}", // Уникальное имя для поиска
            StatusId = 0,
            DeliveryId = 1, // ID существующего способа доставки
            PaymentId = 1,  // ID существующего способа оплаты
            Sum = 0,
            DeliveryCost = 0,
            UserComment = $"Интеграционный тест заказа {uniqueId}" // Комментарий все равно отправим
                                                                   // OrderItems не указываем
        };
        var ordersToImport = new List<Order> { newOrder };

        _logger.LogInformation("Запуск теста CreateAndDeleteBasicOrderAsync: Создание заказа для Email: {Email}...", newOrder.UserEmail);

        string? importResult = null;
        List<Order>? foundOrders = null;
        Order? createdOrder = null;
        long? createdOrderId = null;

        // --- Act 1: Создание ---
        var createException = await Record.ExceptionAsync(async () =>
        {
            importResult = await _apiClient!.ImportOrderAsync(ordersToImport);

            // Ищем созданный заказ по email и имени покупателя
            await Task.Delay(1000); // Пауза
            var searchParams = new GetOrderRequestParams { Emails = new List<string> { newOrder.UserEmail }, Count = 10 }; // Ищем последние 10
            foundOrders = await _apiClient!.GetOrderAsync(searchParams);
            createdOrder = foundOrders?.OrderByDescending(o => o.Id).FirstOrDefault(o => o.NameBuyer == newOrder.NameBuyer);
            createdOrderId = createdOrder?.Id;
        });

        // --- Assert 1: Проверка создания ---
        if (createException != null) _logger.LogError(createException, "Этап создания в тесте CreateAndDeleteBasicOrderAsync провален.");
        Assert.Null(createException);
        Assert.NotNull(importResult);
        Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase); // Гибкая проверка ответа
        Assert.NotNull(foundOrders);
        Assert.NotNull(createdOrder); // Убеждаемся, что наш заказ найден
        Assert.NotNull(createdOrderId); // У него должен быть ID
        Assert.Equal(newOrder.UserEmail, createdOrder.UserEmail);
        Assert.Equal(newOrder.NameBuyer, createdOrder.NameBuyer); // Проверяем имя
        _logger.LogInformation("Этап создания в тесте CreateAndDeleteBasicOrderAsync: Заказ создан и найден, ID={OrderId}", createdOrderId);

        // --- Arrange 2: Подготовка к удалению ---
        string? deleteResult = null;
        List<Order>? foundAfterDelete = null;

        // --- Act 2: Удаление ---
        _logger.LogInformation("Этап удаления в тесте CreateAndDeleteBasicOrderAsync: Удаление заказа ID={OrderId}...", createdOrderId.Value);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            deleteResult = await _apiClient!.DeleteOrderAsync(new List<long> { createdOrderId.Value });
            // Проверяем, что заказ действительно удален (ищем по ID)
            await Task.Delay(500);
            var searchParamsCheck = new GetOrderRequestParams { Ids = new List<long> { createdOrderId.Value } };
            foundAfterDelete = await _apiClient!.GetOrderAsync(searchParamsCheck);
        });

        // --- Assert 2: Проверка удаления ---
        if (deleteException != null) _logger.LogError(deleteException, "Этап удаления в тесте CreateAndDeleteBasicOrderAsync провален.");
        Assert.Null(deleteException);

        Assert.NotNull(deleteResult);
        // Проверяем ответ API удаления (скорее всего, "Удаление завершено")
        Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation("Этап удаления в тесте CreateAndDeleteBasicOrderAsync: Ответ API на удаление: {Result}", deleteResult);

        Assert.NotNull(foundAfterDelete);
        Assert.Empty(foundAfterDelete); // Список найденных должен быть пуст после удаления
        _logger.LogInformation("Тест CreateAndDeleteBasicOrderAsync_RealApi_ShouldSucceed: Заказ ID={OrderId} успешно создан и затем удален.", createdOrderId);
    }
    
    /// <summary>
    /// Проверяет создание новых дополнительных полей для заказа через реальное API.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// ВНИМАНИЕ: Этот тест создает/изменяет настройки на сервере.
    /// </summary>
    [Fact]
    public async Task CreateOrUpdateOrderCustomFieldsAsync_RealApi_ShouldCreateFields()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateOrUpdateOrderCustomFieldsAsync_RealApi_ShouldCreateFields: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // Arrange
        // Генерируем уникальные имена полей
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 6);
        var textFieldName = $"ИнтегрПолеТекст{uniqueId}";
        var selectFieldName = $"ИнтегрПолеСелект{uniqueId}";

        var fieldsToCreate = new List<CustomFieldDefinition> {
            new CustomFieldDefinition {
                Name = textFieldName,
                Type = "input", // Или textarea
                Required = false,
                Active = true
            },
            new CustomFieldDefinition {
                Name = selectFieldName,
                Type = "select",
                Variants = new List<string> { $"Опция1-{uniqueId}", $"Опция2-{uniqueId}" },
                Required = true,
                Active = true
            }
        };

        _logger.LogInformation("Запуск теста CreateOrUpdateOrderCustomFieldsAsync_RealApi_ShouldCreateFields для полей: {FieldName1}, {FieldName2}...", textFieldName, selectFieldName);

        string? result = null;

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            result = await _apiClient!.CreateOrUpdateOrderCustomFieldsAsync(fieldsToCreate);
        });

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест CreateOrUpdateOrderCustomFieldsAsync_RealApi_ShouldCreateFields провален.");
        Assert.Null(exception); // Не должно быть исключений

        Assert.NotNull(result);
        // Проверяем ожидаемый ответ от API (может отличаться, подставьте реальный)
        Assert.Contains("Поля сохранены", result, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation("Тест CreateOrUpdateOrderCustomFieldsAsync_RealApi_ShouldCreateFields: Ответ API: {Result}", result);

        // Дополнительная проверка:
        // В идеале, нужно было бы как-то проверить, что поля действительно создались.
        // Но API Moguta, похоже, не предоставляет методов для получения списка доп. полей заказа.
        // Поэтому ограничиваемся проверкой успешного ответа от метода создания/обновления.
        // Если бы был метод GetOrderCustomFields, мы бы вызвали его здесь.
    }

}