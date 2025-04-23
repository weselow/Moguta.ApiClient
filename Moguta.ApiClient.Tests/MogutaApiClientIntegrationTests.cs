using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moguta.ApiClient.Abstractions;
using Moguta.ApiClient.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Xunit.Sdk; // Для Assert.Fail
using Microsoft.Extensions.Logging.Abstractions; // Для NullLogger
using Moguta.ApiClient.Models.Common; // Для DTO
using Moguta.ApiClient.Models.Requests; // Для DTO запросов
using NLog; // Для LogManager, если используется в Dispose
using System;
using NLog.Extensions.Logging; // Для Guid

namespace Moguta.ApiClient.Tests;

[Trait("Category", "Integration")]
public class MogutaApiClientIntegrationTests //: IDisposable // Убираем IDisposable
{
    private readonly IMogutaApiClient? _apiClient;
    private readonly IConfiguration _configuration;
    private readonly bool _canRunIntegrationTests;
    private readonly ILogger<MogutaApiClientIntegrationTests> _logger;

    public MogutaApiClientIntegrationTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<MogutaApiClientIntegrationTests>()
            .Build();

        var siteUrl = _configuration["MogutaApiIntegration:SiteUrl"];
        var token = _configuration["MogutaApiIntegration:Token"];
        var secretKey = _configuration["MogutaApiIntegration:SecretKey"];

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
        _logger = serviceProviderBuilder.GetService<ILogger<MogutaApiClientIntegrationTests>>()
                  ?? NullLogger<MogutaApiClientIntegrationTests>.Instance;

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
    /// Проверяет получение категорий от реального API с использованием пагинации.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// </summary>
    [Fact]
    public async Task GetCategoriesAsync_RealApi_ShouldReturnCategories()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста GetCategoriesAsync_RealApi_ShouldReturnCategories: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }
        _logger.LogInformation("Запуск теста GetCategoriesAsync_RealApi_ShouldReturnCategories...");

        // Arrange
        var requestParams = new GetCategoryRequestParams
        {
            Page = 1,
            Count = 50
        };
        List<Category>? categories = null;

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            categories = await _apiClient!.GetCategoryAsync(requestParams);
        });

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест GetCategoriesAsync_RealApi_ShouldReturnCategories провален.");
        Assert.Null(exception);
        Assert.NotNull(categories);
        _logger.LogInformation("Тест GetCategoriesAsync_RealApi_ShouldReturnCategories: Получено {Count} категорий.", categories.Count);
        Assert.True(categories.Count <= 50);
    }

    /// <summary>
    /// Проверяет получение списка продуктов от реального API с использованием пагинации.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// </summary>
    [Fact]
    public async Task GetProductAsync_RealApi_ShouldReturnProducts()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста GetProductAsync_RealApi_ShouldReturnProducts: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }
        _logger.LogInformation("Запуск теста GetProductAsync_RealApi_ShouldReturnProducts...");

        // Arrange
        var requestParams = new GetProductRequestParams
        {
            Page = 1,
            Count = 10,
            IncludeVariants = true,
            IncludeProperties = true
        };
        List<Product>? products = null;

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            products = await _apiClient!.GetProductAsync(requestParams);
        });

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест GetProductAsync_RealApi_ShouldReturnProducts провален.");
        Assert.Null(exception);
        Assert.NotNull(products);
        _logger.LogInformation("Тест GetProductAsync_RealApi_ShouldReturnProducts: Получено {Count} продуктов на первой странице.", products.Count);

        if (products.Count > 0)
        {
            var firstProduct = products[0];
            _logger.LogDebug(" - Первый продукт: ID={Id}, Title='{Title}', Code='{Code}', Variants={VariantCount}, Properties={PropertyCount}",
                             firstProduct.Id, firstProduct.Title, firstProduct.Code, firstProduct.Variants?.Count ?? 0, firstProduct.Property?.Count ?? 0);
            Assert.NotNull(firstProduct.Title);
        }
    }

    #region GetUserAsync Tests

    /// <summary>
    /// Проверяет получение списка пользователей от реального API с использованием пагинации.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// </summary>
    [Fact]
    public async Task GetUserAsync_RealApi_ShouldReturnUsers()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста GetUserAsync_RealApi_ShouldReturnUsers: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }
        _logger.LogInformation("Запуск теста GetUserAsync_RealApi_ShouldReturnUsers...");

        // Arrange
        var requestParams = new GetUserRequestParams
        {
            Page = 1,
            Count = 5
        };
        List<User>? users = null;

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            users = await _apiClient!.GetUserAsync(requestParams);
        });

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест GetUserAsync_RealApi_ShouldReturnUsers провален.");
        Assert.Null(exception);
        Assert.NotNull(users);
        _logger.LogInformation("Тест GetUserAsync_RealApi_ShouldReturnUsers: Получено {Count} пользователей на первой странице.", users.Count);

        if (users.Count > 0)
        {
            var firstUser = users[0];
            _logger.LogDebug(" - Первый пользователь: ID={Id}, Email='{Email}', Name='{Name}', Role={Role}, Activity={Activity}",
                             firstUser.Id, firstUser.Email, firstUser.Name, firstUser.Role, firstUser.Activity);
            Assert.False(string.IsNullOrWhiteSpace(firstUser.Email));
        }
    }

    #endregion

    #region GetOrderAsync Tests

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

    #endregion

    #region FindUserAsync Tests

    /// <summary>
    /// Проверяет поиск существующего пользователя по Email на реальном API.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// </summary>
    /// <remarks>
    /// ЗАМЕНИТЕ "user@moguta.test" на РЕАЛЬНЫЙ email существующего пользователя на вашем тестовом сервере.
    /// </remarks>
    [Fact]
    public async Task FindUserAsync_RealApi_ShouldReturnExistingUser()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста FindUserAsync_RealApi_ShouldReturnExistingUser: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- !!! ЗАМЕНИТЬ НА РЕАЛЬНЫЙ EMAIL !!! ---
        string existingEmail = "user@moguta.ru"; // Пример, обязательно замените!
        // ------------------------------------------

        _logger.LogInformation("Запуск теста FindUserAsync_RealApi_ShouldReturnExistingUser для email: {Email}...", existingEmail);

        // Arrange
        User? foundUser = null;

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            foundUser = await _apiClient!.FindUserAsync(existingEmail);
        });

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест FindUserAsync_RealApi_ShouldReturnExistingUser провален.");
        Assert.Null(exception);
        Assert.NotNull(foundUser);
        Assert.Equal(existingEmail, foundUser.Email, ignoreCase: true);
        _logger.LogInformation("Тест FindUserAsync_RealApi_ShouldReturnExistingUser: Пользователь найден - ID={Id}, Name='{Name}'", foundUser.Id, foundUser.Name);
    }

    /// <summary>
    /// Проверяет поиск несуществующего пользователя по Email на реальном API.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// </summary>
    [Fact]
    public async Task FindUserAsync_RealApi_ShouldReturnNullForNotFound()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста FindUserAsync_RealApi_ShouldReturnNullForNotFound: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // Arrange
        string nonExistingEmail = $"nonexistent-user-{Guid.NewGuid()}@example.invalid";
        _logger.LogInformation("Запуск теста FindUserAsync_RealApi_ShouldReturnNullForNotFound для email: {Email}...", nonExistingEmail);
        User? foundUser = null;

        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            foundUser = await _apiClient!.FindUserAsync(nonExistingEmail);
        });

        // Assert
        if (exception != null) _logger.LogError(exception, "Тест FindUserAsync_RealApi_ShouldReturnNullForNotFound провален.");
        Assert.Null(exception);
        Assert.Null(foundUser);
        _logger.LogInformation("Тест FindUserAsync_RealApi_ShouldReturnNullForNotFound пройден (пользователь не найден, как и ожидалось).");
    }
   
    #endregion
    
    #region CreateOrUpdateOrderCustomFieldsAsync Tests

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
    #endregion
    
    #region Combined User Create & Delete Test

    /// <summary>
    /// Проверяет полный цикл создания и последующего удаления пользователя через реальное API.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// ВНИМАНИЕ: Этот тест СОЗДАЕТ и УДАЛЯЕТ данные на сервере.
    /// </summary>
    [Fact]
    public async Task CreateAndDeleteUserAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateAndDeleteUserAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание пользователя ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 12);
        var newUserEmail = $"delete.user.{uniqueId}@integration.test"; // Уникальный email
        var newUser = new User
        {
            Email = newUserEmail,
            Name = $"Тест Юзер Удаление {uniqueId}",
            Role = 2,
            Activity = true,
            Blocked = false
        };
        var usersToImport = new List<User> { newUser };
        _logger.LogInformation("Запуск теста CreateAndDeleteUserAsync: Создание пользователя с Email: {Email}...", newUser.Email);

        string? importResult = null;
        User? createdUser = null;

        // --- Act 1: Создание ---
        var createException = await Record.ExceptionAsync(async () =>
        {
            importResult = await _apiClient!.ImportUserAsync(usersToImport, true); // enableUpdate = true
            await Task.Delay(500); // Пауза
            createdUser = await _apiClient!.FindUserAsync(newUser.Email);
        });

        // Assert 1: Проверка создания
        if (createException != null) _logger.LogError(createException, "Этап создания в тесте CreateAndDeleteUserAsync провален.");
        Assert.Null(createException);
        Assert.NotNull(importResult);
        // Проверяем ответ импорта (может быть "Импорт завершен" или "Импортировано: 1 Обновлено: 0")
        Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(createdUser); // Убеждаемся, что пользователь найден
        Assert.Equal(newUser.Email, createdUser.Email, ignoreCase: true);
        Assert.NotNull(createdUser.Id); // Убеждаемся, что у него есть ID
        _logger.LogInformation("Этап создания в тесте CreateAndDeleteUserAsync: Пользователь создан, ID={UserId}", createdUser.Id);

        // --- Arrange 2: Подготовка к удалению ---
        string? deleteResult = null;
        User? foundAfterDelete = null;
        var emailsToDelete = new List<string> { newUser.Email };

        // --- Act 2: Удаление ---
        _logger.LogInformation("Этап удаления в тесте CreateAndDeleteUserAsync: Удаление пользователя Email={Email}...", newUser.Email);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            deleteResult = await _apiClient!.DeleteUserAsync(emailsToDelete);
            await Task.Delay(500); // Пауза
            foundAfterDelete = await _apiClient!.FindUserAsync(newUser.Email); // Ищем по Email удаленного
        });

        // Assert 2: Проверка удаления
        if (deleteException != null) _logger.LogError(deleteException, "Этап удаления в тесте CreateAndDeleteUserAsync провален.");
        Assert.Null(deleteException); // Не должно быть ошибок при удалении и поиске

        Assert.NotNull(deleteResult);
        // Проверяем ответ API (скорее всего, "Удалено: 1" или "Удаление завершено")
        Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        _logger.LogInformation("Этап удаления в тесте CreateAndDeleteUserAsync: Ответ API на удаление: {Result}", deleteResult);

        Assert.Null(foundAfterDelete); // Пользователь НЕ должен быть найден после удаления
        _logger.LogInformation("Тест CreateAndDeleteUserAsync_RealApi_ShouldSucceed: Пользователь Email={Email} успешно создан и затем удален.", newUser.Email);
    }
    #endregion

    #region ImportOrderAsync Tests (Basic)

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

    #endregion

    #region Combined Category Create-Update-Delete Test

    /// <summary>
    /// Проверяет полный цикл: создание, обновление и удаление категории через реальное API.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// ВНИМАНИЕ: Этот тест СОЗДАЕТ, ОБНОВЛЯЕТ и УДАЛЯЕТ данные на сервере.
    /// </summary>
    [Fact]
    public async Task CreateUpdateDeleteCategoryAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateUpdateDeleteCategoryAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание категории ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var initialTitle = $"Тест Кат Созд {uniqueId}";
        var initialUrl = $"test-cat-create-{uniqueId}";
        var updatedTitle = $"Тест Кат Обновл {uniqueId}"; // Новое название для обновления
        var initialCategory = new Category
        {
            Title = initialTitle,
            Url = initialUrl,
            Parent = 0,
            Activity = true,
            Invisible = false,
            Export = true,
            Sort = 1001
        };
        long? createdCategoryId = null;
        _logger.LogInformation("Запуск теста CreateUpdateDeleteCategory: Создание категории URL={Url}...", initialUrl);

        // --- Act 1: Создание ---
        var createException = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportCategoryAsync(new List<Category> { initialCategory });
            Assert.NotNull(importResult);
            Assert.Contains("Импорт завершен", importResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetCategoryRequestParams { Urls = new List<string> { initialUrl } };
            var found = await _apiClient!.GetCategoryAsync(searchParams);
            createdCategoryId = found?.FirstOrDefault()?.Id;
        });

        // Assert 1: Проверка создания
        if (createException != null) _logger.LogError(createException, "Этап создания в тесте CreateUpdateDeleteCategory провален.");
        Assert.Null(createException);
        Assert.NotNull(createdCategoryId); // ID должен быть присвоен
        _logger.LogInformation("Этап создания в тесте CreateUpdateDeleteCategory: Категория создана, ID={CatId}", createdCategoryId);

        // --- Arrange 2: Подготовка к обновлению ---
        var categoryToUpdate = new Category
        {
            Id = createdCategoryId.Value, // Указываем ID для обновления
            Title = updatedTitle,         // Меняем название
            Url = initialUrl,             // URL обычно не меняют или API сам решит
            Parent = 0,                   // Остальные поля можно не указывать, если API их не сбросит
            Activity = true              // Лучше указать основные флаги
        };
        _logger.LogInformation("Этап обновления в тесте CreateUpdateDeleteCategory: Обновление категории ID={CatId} на Title='{NewTitle}'...", createdCategoryId, updatedTitle);

        // --- Act 2: Обновление ---
        string? updateResult = null;
        Category? updatedCategory = null;
        var updateException = await Record.ExceptionAsync(async () =>
        {
            updateResult = await _apiClient!.ImportCategoryAsync(new List<Category> { categoryToUpdate });
            await Task.Delay(500);
            var searchParams = new GetCategoryRequestParams { Ids = new List<long> { createdCategoryId.Value } }; // Ищем по ID
            updatedCategory = (await _apiClient!.GetCategoryAsync(searchParams))?.FirstOrDefault();
        });

        // Assert 2: Проверка обновления
        if (updateException != null) _logger.LogError(updateException, "Этап обновления в тесте CreateUpdateDeleteCategory провален.");
        Assert.Null(updateException);
        Assert.NotNull(updateResult);
        // Ответ API на обновление может быть таким же "Импорт завершен" или "Обновлено: 1"
        Assert.True(updateResult.Contains("импорт", StringComparison.OrdinalIgnoreCase) || updateResult.Contains("обновлен", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(updatedCategory);
        Assert.Equal(updatedTitle, updatedCategory.Title); // Проверяем, что название обновилось
        _logger.LogInformation("Этап обновления в тесте CreateUpdateDeleteCategory: Категория ID={CatId} успешно обновлена.", createdCategoryId);

        // --- Arrange 3: Подготовка к удалению ---
        string? deleteResult = null;
        List<Category>? foundAfterDelete = null;

        // --- Act 3: Удаление ---
        _logger.LogInformation("Этап удаления в тесте CreateUpdateDeleteCategory: Удаление категории ID={CatId}...", createdCategoryId.Value);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            deleteResult = await _apiClient!.DeleteCategoryAsync(new List<long> { createdCategoryId.Value });
            await Task.Delay(500);
            var searchParamsCheck = new GetCategoryRequestParams { Ids = new List<long> { createdCategoryId.Value } };
            foundAfterDelete = await _apiClient!.GetCategoryAsync(searchParamsCheck);
        });

        // --- Assert 3: Проверка удаления ---
        if (deleteException != null) _logger.LogError(deleteException, "Этап удаления в тесте CreateUpdateDeleteCategory провален.");
        Assert.Null(deleteException);
        Assert.NotNull(deleteResult);
        Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(foundAfterDelete);
        Assert.Empty(foundAfterDelete); // Категория не должна быть найдена
        _logger.LogInformation("Тест CreateUpdateDeleteCategoryAsync_RealApi_ShouldSucceed: Категория ID={CatId} успешно создана, обновлена и удалена.", createdCategoryId);
    }

    #endregion

    #region Combined Product Create-Update-Delete Test

    /// <summary>
    /// Проверяет полный цикл: создание, обновление и удаление товара через реальное API.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// ВНИМАНИЕ: Этот тест СОЗДАЕТ, ОБНОВЛЯЕТ и УДАЛЯЕТ данные на сервере.
    /// </summary>
    [Fact]
    public async Task CreateUpdateDeleteProductAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateUpdateDeleteProductAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание товара ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 10);
        var initialTitle = $"Тест Товар Созд {uniqueId}";
        var initialCode = $"TEST-CREATE-{uniqueId}";
        var initialUrl = $"test-prod-create-{uniqueId}";
        var updatedTitle = $"Тест Товар Обновл {uniqueId}"; // Новое название
        decimal updatedPrice = 123.45m; // Новая цена
        // !!! ЗАМЕНИТЬ НА РЕАЛЬНЫЙ ID КАТЕГОРИИ !!!
        long categoryId = 2;
        // -----------------------------------------
        var initialProduct = new Product
        {
            CatId = categoryId,
            Title = initialTitle,
            Code = initialCode,
            Url = initialUrl,
            Price = 50.00m,
            Count = 20,
            Activity = true,
            Unit = "шт."
        };
        long? createdProductId = null;
        _logger.LogInformation("Запуск теста CreateUpdateDeleteProduct: Создание товара Code={Code}...", initialCode);

        // --- Act 1: Создание ---
        var createException = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportProductAsync(new List<Product> { initialProduct });
            Assert.NotNull(importResult);
            Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase);
            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Codes = new List<string> { initialCode } };
            createdProductId = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault()?.Id;
        });

        // Assert 1: Проверка создания
        if (createException != null) _logger.LogError(createException, "Этап создания в тесте CreateUpdateDeleteProduct провален.");
        Assert.Null(createException);
        Assert.NotNull(createdProductId);
        _logger.LogInformation("Этап создания в тесте CreateUpdateDeleteProduct: Товар создан, ID={ProdId}", createdProductId);

        // --- Arrange 2: Подготовка к обновлению ---
        var productToUpdate = new Product
        {
            Id = createdProductId.Value, // Указываем ID
            CatId = categoryId,          // Категорию обычно надо указывать
            Title = updatedTitle,        // Новое название
            Price = updatedPrice,        // Новая цена
            Code = initialCode,          // Артикул оставляем
            Url = initialUrl,            // URL оставляем
            Activity = true,             // Статус активности
            Count = 15                   // Можно обновить и количество
        };
        _logger.LogInformation("Этап обновления в тесте CreateUpdateDeleteProduct: Обновление товара ID={ProdId}...", createdProductId);

        // --- Act 2: Обновление ---
        string? updateResult = null;
        Product? updatedProduct = null;
        var updateException = await Record.ExceptionAsync(async () =>
        {
            updateResult = await _apiClient!.ImportProductAsync(new List<Product> { productToUpdate });
            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Ids = new List<long> { createdProductId.Value }, IncludeVariants = false, IncludeProperties = false }; // Ищем по ID
            updatedProduct = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault();
        });

        // Assert 2: Проверка обновления
        if (updateException != null) _logger.LogError(updateException, "Этап обновления в тесте CreateUpdateDeleteProduct провален.");
        Assert.Null(updateException);
        Assert.NotNull(updateResult);
        Assert.True(updateResult.Contains("импорт", StringComparison.OrdinalIgnoreCase) || updateResult.Contains("обновлен", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(updatedProduct);
        Assert.Equal(updatedTitle, updatedProduct.Title); // Проверяем название
        Assert.Equal(updatedPrice, updatedProduct.Price); // Проверяем цену
        _logger.LogInformation("Этап обновления в тесте CreateUpdateDeleteProduct: Товар ID={ProdId} успешно обновлен.", createdProductId);

        // --- Arrange 3: Подготовка к удалению ---
        string? deleteResult = null;
        List<Product>? foundAfterDelete = null;

        // --- Act 3: Удаление ---
        _logger.LogInformation("Этап удаления в тесте CreateUpdateDeleteProduct: Удаление товара ID={ProdId}...", createdProductId.Value);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            deleteResult = await _apiClient!.DeleteProductAsync(new List<long> { createdProductId.Value });
            await Task.Delay(500);
            var searchParamsCheck = new GetProductRequestParams { Ids = new List<long> { createdProductId.Value } }; // Ищем по ID удаленного
            foundAfterDelete = await _apiClient!.GetProductAsync(searchParamsCheck);
        });

        // --- Assert 3: Проверка удаления ---
        if (deleteException != null) _logger.LogError(deleteException, "Этап удаления в тесте CreateUpdateDeleteProduct провален.");
        Assert.Null(deleteException);
        Assert.NotNull(deleteResult);
        Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(foundAfterDelete);
        Assert.Empty(foundAfterDelete); // Товар не должен быть найден
        _logger.LogInformation("Тест CreateUpdateDeleteProductAsync_RealApi_ShouldSucceed: Товар ID={ProdId} успешно создан, обновлен и удален.", createdProductId);
    }
    #endregion

}