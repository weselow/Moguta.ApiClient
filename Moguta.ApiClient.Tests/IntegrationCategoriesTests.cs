﻿using Microsoft.Extensions.Configuration;
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

[Trait("Category", "Integration")]
public class IntegrationCategoriesTests
{
    private readonly IMogutaApiClient? _apiClient;
    private readonly bool _canRunIntegrationTests;
    private readonly ILogger<IntegrationCategoriesTests> _logger;

    public IntegrationCategoriesTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<IntegrationCategoriesTests>()
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
        _logger = serviceProviderBuilder.GetService<ILogger<IntegrationCategoriesTests>>()
                  ?? NullLogger<IntegrationCategoriesTests>.Instance;

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
        var requestParams = new GetMogutaCategoryRequestParams
        {
            Page = 1,
            Count = 50
        };
        List<MogutaCategory>? categories = null;

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
    /// Получаем категории, когда в запросе Count = null.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetCategoriesAsync_RealApi_WithNullCount()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста GetCategoriesAsync_RealApi_ShouldReturnCategories: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }
        _logger.LogInformation("Запуск теста GetCategoriesAsync_RealApi_ShouldReturnCategories...");

        // Arrange
        var requestParams = new GetMogutaCategoryRequestParams
        {
            Page = 1,
            //Count = 50
        };
        List<MogutaCategory>? categories = null;

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
    }

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
        var initialCategory = new MogutaCategory
        {
            Title = initialTitle,
            Url = initialUrl,
            Parent = 13,
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
            var importResult = await _apiClient!.ImportCategoryAsync(new List<MogutaCategory> { initialCategory });
            Assert.NotNull(importResult);
            Assert.Contains("Импорт завершен", importResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetMogutaCategoryRequestParams { Urls = new List<string> { initialUrl } };
            var found = await _apiClient!.GetCategoryAsync(searchParams);
            createdCategoryId = found?.FirstOrDefault()?.Id;
        });

        // Assert 1: Проверка создания
        if (createException != null) _logger.LogError(createException, "Этап создания в тесте CreateUpdateDeleteCategory провален.");
        Assert.Null(createException);
        Assert.NotNull(createdCategoryId); // ID должен быть присвоен
        _logger.LogInformation("Этап создания в тесте CreateUpdateDeleteCategory: Категория создана, ID={CatId}", createdCategoryId);

        // --- Arrange 2: Подготовка к обновлению ---
        var categoryToUpdate = new MogutaCategory
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
        MogutaCategory? updatedCategory = null;
        var updateException = await Record.ExceptionAsync(async () =>
        {
            updateResult = await _apiClient!.ImportCategoryAsync(new List<MogutaCategory> { categoryToUpdate });
            await Task.Delay(500);
            var searchParams = new GetMogutaCategoryRequestParams { Ids = new List<long> { createdCategoryId.Value } }; // Ищем по ID
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
        List<MogutaCategory>? foundAfterDelete = null;

        // --- Act 3: Удаление ---
        _logger.LogInformation("Этап удаления в тесте CreateUpdateDeleteCategory: Удаление категории ID={CatId}...", createdCategoryId.Value);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            deleteResult = await _apiClient!.DeleteCategoryAsync(new List<long> { createdCategoryId.Value });
            await Task.Delay(500);
            var searchParamsCheck = new GetMogutaCategoryRequestParams { Ids = new List<long> { createdCategoryId.Value } };
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

    /// <summary>
    /// Удаляем все категории с сайта.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteAllCategoriesAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateUpdateDeleteCategoryAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }
        // --- Arrange: Создание категории ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var initialCategory = new MogutaCategory
        {
            Title = $"Тест Кат Созд {uniqueId}",
            Url = $"test-cat-create-{uniqueId}",
            Parent = 0,
            Activity = true,
            Invisible = false,
            Export = true,
            Sort = 1001
        };
        var createException = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportCategoryAsync([initialCategory]);
            Assert.NotNull(importResult);
            Assert.Contains("Импорт завершен", importResult, StringComparison.OrdinalIgnoreCase);
            await Task.Delay(500);
        });
        Assert.Null(createException);

        string? deleteResult = null;
        var totalDelted = 0;
        List<MogutaCategory>? foundAfterDelete = null;
        var searchParams = new GetMogutaCategoryRequestParams
        {
            Page = 1
        };  

        // --- Act 1: Удаление ---
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            while (true)
            {
                foundAfterDelete = await _apiClient!.GetCategoryAsync(searchParams);
                if (foundAfterDelete == null || foundAfterDelete.Count == 0) break;
                var ids = foundAfterDelete.Where(t => t.Id != null).Select(t => t.Id ?? 0).ToList();
                deleteResult = await _apiClient!.DeleteCategoryAsync(ids);
                totalDelted += ids.Count;
                _logger.LogInformation("... удалено {amount} категорий / всего удалено {total} категорий ", ids.Count, totalDelted);
                await Task.Delay(500);
            } 
        });

        // --- Assert 3: Проверка удаления ---
        if (deleteException != null) _logger.LogError(deleteException, "Этап удаления в тесте CreateUpdateDeleteCategory провален.");
        Assert.Null(deleteException);
        Assert.NotNull(deleteResult);
        Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(foundAfterDelete);
        Assert.Empty(foundAfterDelete);
        _logger.LogInformation("Тест DeleteAllCategoriesAsync_RealApi_ShouldSucceed пройден.");
    }

    /// <summary>
    /// Проверяет, умеет ли API сопоставлять категории по имени.
    /// Создает категорию, изменяет поле (кроме имени), отправляет повторно и проверяет количество категорий с этим именем.
    /// </summary>
    [Fact]
    public async Task MatchCategoryByNameAsync_RealApi_ShouldHandleDuplicateNames()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста MatchCategoryByNameAsync_RealApi_ShouldHandleDuplicateNames: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание категории ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var categoryName = $"Тест Категория {uniqueId}";
        var categoryUrl = $"test-cat-{uniqueId}";
        var initialCategory = new MogutaCategory
        {
            Title = categoryName,
            Url = categoryUrl,
            Parent = 0,
            Activity = true,
            Invisible = false,
            Export = true,
            Sort = 1001
        };

        // --- Act 1: Создание категории ---
        _logger.LogInformation("Создание категории с именем '{Name}'...", categoryName);
        var createException1 = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportCategoryAsync(new List<MogutaCategory> { initialCategory });
            Assert.NotNull(importResult);
            Assert.Contains("Импорт завершен", importResult, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Null(createException1);

        // --- Act 2: Изменение поля и повторное создание ---
        initialCategory.Sort = 2002; // Изменяем поле Sort
        _logger.LogInformation("Повторное создание категории с измененным полем Sort...");
        var createException2 = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportCategoryAsync(new List<MogutaCategory> { initialCategory });
            Assert.NotNull(importResult);
            Assert.Contains("Импорт завершен", importResult, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Null(createException2);

        // --- Act 3: Запрос категорий по имени ---
        _logger.LogInformation("Запрос категорий с именем '{Name}' по url...", categoryName);
        List<MogutaCategory>? matchingCategories = null;
        var fetchException = await Record.ExceptionAsync(async () =>
        {
            var requestParams = new GetMogutaCategoryRequestParams
            {
                Urls = new List<string> { categoryUrl }
            };
            matchingCategories = await _apiClient!.GetCategoryAsync(requestParams);
        });
        Assert.Null(fetchException);

        // --- Assert: Проверка количества категорий ---
        Assert.NotNull(matchingCategories);
        Assert.Single(matchingCategories); // Ожидаем, что категория одна
        _logger.LogInformation("Тест MatchCategoryByNameAsync_RealApi_ShouldHandleDuplicateNames успешно пройден.");

        // --- Cleanup: Удаление созданной категории ---
        _logger.LogInformation("Удаление созданной категории...");
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            var idsToDelete = matchingCategories!.Select(c => c.Id!.Value).ToList();
            var deleteResult = await _apiClient!.DeleteCategoryAsync(idsToDelete);
            Assert.NotNull(deleteResult);
            Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Null(deleteException);
    }
}