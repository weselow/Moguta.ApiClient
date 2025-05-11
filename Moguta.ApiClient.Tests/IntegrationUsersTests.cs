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

[Trait("Users", "Integration")]
public class IntegrationUsersTests
{
    private readonly IMogutaApiClient? _apiClient;
    private readonly bool _canRunIntegrationTests;
    private readonly ILogger<IntegrationUsersTests> _logger;

    public IntegrationUsersTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<IntegrationUsersTests>()
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
        _logger = serviceProviderBuilder.GetService<ILogger<IntegrationUsersTests>>()
                  ?? NullLogger<IntegrationUsersTests>.Instance;

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
        List<MogutaUser>? users = null;

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
        string existingEmail = "mail@server360.ru"; // Пример, обязательно замените!
        // ------------------------------------------

        _logger.LogInformation("Запуск теста FindUserAsync_RealApi_ShouldReturnExistingUser для email: {Email}...", existingEmail);

        // Arrange
        MogutaUser? foundUser = null;

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
        MogutaUser? foundUser = null;

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
        var newUser = new MogutaUser
        {
            Email = newUserEmail,
            Name = $"Тест Юзер Удаление {uniqueId}",
            Role = 2,
            Activity = true,
            Blocked = false
        };
        var usersToImport = new List<MogutaUser> { newUser };
        _logger.LogInformation("Запуск теста CreateAndDeleteUserAsync: Создание пользователя с Email: {Email}...", newUser.Email);

        string? importResult = null;
        MogutaUser? createdUser = null;

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
        MogutaUser? foundAfterDelete = null;
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
    
}