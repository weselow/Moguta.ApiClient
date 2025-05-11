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

[Trait("Product", "Integration")]
public class IntegrationProductsTests
{
    private readonly IMogutaApiClient? _apiClient;
    private readonly bool _canRunIntegrationTests;
    private readonly ILogger<IntegrationProductsTests> _logger;

    public IntegrationProductsTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<IntegrationProductsTests>()
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
        _logger = serviceProviderBuilder.GetService<ILogger<IntegrationProductsTests>>()
                  ?? NullLogger<IntegrationProductsTests>.Instance;

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
        List<MogutaProduct>? products = null;

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
        long categoryId = 1;
        // -----------------------------------------
        var initialProduct = new MogutaProduct
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
            var importResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { initialProduct });
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
        var productToUpdate = new MogutaProduct
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
        MogutaProduct? updatedProduct = null;
        var updateException = await Record.ExceptionAsync(async () =>
        {
            updateResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { productToUpdate });
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
        List<MogutaProduct>? foundAfterDelete = null;

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


    /// <summary>
    /// Проверяет полный цикл создания "реалистичного" товара (с вариантами и характеристиками)
    /// и его последующего удаления через реальное API.
    /// Тест будет пропущен, если конфигурация не найдена.
    /// ВНИМАНИЕ: Этот тест СОЗДАЕТ и УДАЛЯЕТ данные на сервере.
    /// </summary>
    [Fact]
    public async Task CreateAndDeleteRealisticProductAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateAndDeleteRealisticProductAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Подготовка данных для товара ---
        // !!! ЗАМЕНИТЬ НА РЕАЛЬНЫЙ ID КАТЕГОРИИ (например, "Резисторы") !!!
        long categoryId = 5; // Пример! Укажите ID существующей категории.

        var newProduct = CreateTestProduct(categoryId);
        var productsToImport = new List<MogutaProduct> { newProduct };
        _logger.LogInformation("Запуск теста CreateAndDeleteRealisticProduct: Создание товара Code: {Code}...", newProduct.Code);

        string? importResult = null;
        MogutaProduct? createdProduct = null;
        long? createdProductId = null;

        // --- Act 1: Создание ---
        var createException = await Record.ExceptionAsync(async () =>
        {
            // Добавлять варианты можно только к уже существующему товару.
            importResult = await _apiClient!.ImportProductAsync(productsToImport);
            await Task.Delay(1000);  // Даем больше времени на обработку вариантов/характеристик
            importResult = await _apiClient!.ImportProductAsync(productsToImport);
            await Task.Delay(1000);

            var searchParams = new GetProductRequestParams
            {
                Codes = new List<string> { newProduct.Code },
                IncludeVariants = true, // Запрашиваем варианты для проверки
                IncludeProperties = true // Запрашиваем характеристики для проверки
            };
            createdProduct = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault();
            createdProductId = createdProduct?.Id;
        });

        // Assert 1: Проверка создания
        if (createException != null)
            _logger.LogError(createException,
                "Этап создания в тесте CreateAndDeleteRealisticProduct провален.");

        Assert.Null(createException);
        Assert.NotNull(importResult);
        Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(createdProduct);
        Assert.NotNull(createdProductId);
        Assert.Equal(newProduct.Title, createdProduct.Title);
        Assert.Equal(newProduct.Code, createdProduct.Code);

        // Проверяем, что варианты создались (их количество)
        Assert.NotNull(createdProduct.Variants);
        Assert.Equal(newProduct.Variants?.Count, createdProduct.Variants.Count);

        // Проверяем, что характеристики создались/привязались (количество)
        // API может вернуть больше характеристик, если они унаследованы от категории,
        // поэтому проверяем, что созданные нами присутствуют
        Assert.NotNull(createdProduct.Property);
        Assert.NotEmpty(createdProduct.Property);
        Assert.True(createdProduct.Property.Count >= newProduct.Property?.Count);

        //todo сейчас могута возвращает все характеристики без их значений.
        // когда поправлят - надо будет изменить тест
        //Assert.Contains(createdProduct.Property, p => p.Name == "Сопротивление" && p.Value == "10 кОм");
        //Assert.Contains(createdProduct.Property, p => p.Name == "Мощность" && p.Value == "0.25 Вт");
       
        _logger.LogInformation("Этап создания в тесте CreateAndDeleteRealisticProduct: Товар создан и найден, ID={ProdId}", createdProductId);


        // --- Arrange 2 + Act 2 + Assert 2: Удаление ---
        _logger.LogInformation("Этап удаления в тесте CreateAndDeleteRealisticProduct: Удаление товара ID={ProdId}...", createdProductId.Value);
        string? deleteResult = null;
        List<MogutaProduct>? foundAfterDelete = null;
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            deleteResult = await _apiClient!.DeleteProductAsync(new List<long> { createdProductId.Value });
            await Task.Delay(500);
            var searchParamsCheck = new GetProductRequestParams { Ids = new List<long> { createdProductId.Value } };
            foundAfterDelete = await _apiClient!.GetProductAsync(searchParamsCheck);
        });

        if (deleteException != null) _logger.LogError(deleteException, "Этап удаления в тесте CreateAndDeleteRealisticProduct провален.");
        Assert.Null(deleteException);
        Assert.NotNull(deleteResult);
        Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(foundAfterDelete);
        Assert.Empty(foundAfterDelete);
        _logger.LogInformation("Тест CreateAndDeleteRealisticProductAsync_RealApi_ShouldSucceed: Товар ID={ProdId} успешно создан и затем удален.", createdProductId);
    }

    /// <summary>
    /// Интеграционный тест для создания пачки товаров.
    /// Проверяет создание и удаление нескольких товаров через реальное API.
    /// </summary>
    [Fact]
    public async Task CreateBatchOfProductsAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста CreateBatchOfProductsAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Настройки ---
        int totalProductsToCreate = 200; // Общее количество товаров для создания
        int batchSize = 50; // Размер партии для отправки

        // --- Подготовка ---
        long categoryId = 1; // Пример ID категории
        var allProducts = new List<MogutaProduct>();
        for (int i = 0; i < totalProductsToCreate; i++)
        {
            allProducts.Add(CreateTestProduct(categoryId));
        }

        var createdProductIds = new List<long>();
        _logger.LogInformation("Запуск теста CreateBatchOfProductsAsync: Создание {Total} товаров партиями по {BatchSize}...", totalProductsToCreate, batchSize);

        // --- Создание товаров ---
        Stopwatch sw = new();
        var createException = await Record.ExceptionAsync(async () =>
        {
            for (int i = 0; i < allProducts.Count; i += batchSize)
            {
                var batch = allProducts.Skip(i).Take(batchSize).ToList();
                _logger.LogInformation("Отправляем на создание следующую партию из {amount} товаров.", batch.Count);
                sw.Restart();
                var importResult = await _apiClient!.ImportProductAsync(batch);
                sw.Stop();
                _logger.LogInformation("Партия из {amount} товаров создана за {timer} мс.",
                    batch.Count, sw.ElapsedMilliseconds);
                Assert.NotNull(importResult);
                Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase);

                await Task.Delay(1000); // Пауза для обработки
                var searchParams = new GetProductRequestParams
                {
                    Codes = batch.Select(p => p.Code).ToList(),
                    IncludeVariants = false,
                    IncludeProperties = false
                };
                var createdProducts = await _apiClient!.GetProductAsync(searchParams);
                Assert.NotNull(createdProducts);
                createdProductIds.AddRange(createdProducts!.Select(p => p.Id!.Value));
            }
        });

        // --- Проверка создания ---
        if (createException != null)
            _logger.LogError(createException, "Этап создания в тесте CreateBatchOfProductsAsync провален.");
        Assert.Null(createException);
        Assert.Equal(totalProductsToCreate, createdProductIds.Count);
        _logger.LogInformation("Этап создания в тесте CreateBatchOfProductsAsync: Успешно создано {Count} товаров.", createdProductIds.Count);

        // --- Удаление товаров ---
        _logger.LogInformation("Этап удаления в тесте CreateBatchOfProductsAsync: Удаление {Count} товаров...", createdProductIds.Count);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            var deleteResult = await _apiClient!.DeleteProductAsync(createdProductIds);
            Assert.NotNull(deleteResult);
            Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500); // Пауза для обработки
            var searchParams = new GetProductRequestParams { Ids = createdProductIds };
            var foundAfterDelete = await _apiClient!.GetProductAsync(searchParams);
            Assert.NotNull(foundAfterDelete);
            Assert.Empty(foundAfterDelete);
        });

        // --- Проверка удаления ---
        if (deleteException != null)
            _logger.LogError(deleteException, "Этап удаления в тесте CreateBatchOfProductsAsync провален.");
        Assert.Null(deleteException);
        _logger.LogInformation("Тест CreateBatchOfProductsAsync_RealApi_ShouldSucceed завершен успешно.");
    }


    /// <summary>
    /// Интеграционный тест для проверки обновления товара с использованием ID и Code.
    /// </summary>
    [Fact]
    public async Task UpdateProductWithIdAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста UpdateProductWithIdAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание товара ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var initialTitle = $"Тест Товар {uniqueId}";
        var updatedTitle = $"Тест Товар Обновлен (ID) {uniqueId}";
        var initialCode = $"TEST-CODE-{uniqueId}";
        long categoryId = 1; // Пример ID категории

        var productToCreate = new MogutaProduct
        {
            CatId = categoryId,
            Title = initialTitle,
            Code = initialCode,
            Price = 100.00m,
            Count = 10,
            Activity = true
        };

        long? createdProductId = null;
        _logger.LogInformation("Запуск теста UpdateProductWithIdAsync: Создание товара Code={Code}...", initialCode);

        var createException = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { productToCreate });
            Assert.NotNull(importResult);
            Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Codes = new List<string> { initialCode } };
            createdProductId = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault()?.Id;
        });

        Assert.Null(createException);
        Assert.NotNull(createdProductId);
        _logger.LogInformation("Товар создан, ID={ProdId}", createdProductId);

        // --- Act: Обновление с использованием ID и Code ---
        var productToUpdate = new MogutaProduct
        {
            Id = createdProductId.Value,
            CatId = categoryId,
            Title = updatedTitle,
            Code = initialCode,
            Price = 150.00m,
            Count = 20,
            Activity = true
        };

        MogutaProduct? updatedProduct = null;
        var updateException = await Record.ExceptionAsync(async () =>
        {
            var updateResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { productToUpdate });
            Assert.NotNull(updateResult);
            Assert.Contains("импорт", updateResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Ids = new List<long> { createdProductId.Value } };
            updatedProduct = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault();
        });

        Assert.Null(updateException);
        Assert.NotNull(updatedProduct);
        Assert.Equal(updatedTitle, updatedProduct!.Title);
        Assert.Equal(150.00m, updatedProduct.Price);
        Assert.Equal(20, updatedProduct.Count);
        _logger.LogInformation("Товар успешно обновлен с использованием ID и Code, ID={ProdId}", createdProductId);

        // --- Cleanup: Удаление товара ---
        _logger.LogInformation("Удаление товара ID={ProdId}...", createdProductId);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            var deleteResult = await _apiClient!.DeleteProductAsync(new List<long> { createdProductId.Value });
            Assert.NotNull(deleteResult);
            Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        });

        Assert.Null(deleteException);
        _logger.LogInformation("Товар успешно удален, ID={ProdId}", createdProductId);
    }

    /// <summary>
    /// Интеграционный тест для проверки обновления товара с использованием только Code.
    /// </summary>
    [Fact]
    public async Task UpdateProductWithCodeAsync_RealApi_ShouldSucceed()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста UpdateProductWithCodeAsync_RealApi_ShouldSucceed: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание товара ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var initialTitle = $"Тест Товар {uniqueId}";
        var updatedTitle = $"Тест Товар Обновлен (Code) {uniqueId}";
        var initialCode = $"TEST-CODE-{uniqueId}";
        long categoryId = 1; // Пример ID категории

        var productToCreate = new MogutaProduct
        {
            CatId = categoryId,
            Title = initialTitle,
            Code = initialCode,
            Price = 100.00m,
            Count = 10,
            Activity = true
        };

        long? createdProductId = null;
        _logger.LogInformation("Запуск теста UpdateProductWithCodeAsync: Создание товара Code={Code}...", initialCode);

        var createException = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { productToCreate });
            Assert.NotNull(importResult);
            Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Codes = new List<string> { initialCode } };
            createdProductId = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault()?.Id;
        });

        Assert.Null(createException);
        Assert.NotNull(createdProductId);
        _logger.LogInformation("Товар создан, ID={ProdId}", createdProductId);

        // --- Act: Обновление с использованием только Code ---
        var productToUpdate = new MogutaProduct
        {
            CatId = categoryId,
            Title = updatedTitle,
            Code = initialCode,
            Price = 200.00m,
            Count = 30,
            Activity = true
        };

        MogutaProduct? updatedProduct = null;
        var updateException = await Record.ExceptionAsync(async () =>
        {
            var updateResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { productToUpdate });
            Assert.NotNull(updateResult);
            Assert.Contains("импорт", updateResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Codes = new List<string> { initialCode } };
            updatedProduct = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault();
        });

        Assert.Null(updateException);
        Assert.NotNull(updatedProduct);
        Assert.Equal(updatedTitle, updatedProduct!.Title);
        Assert.Equal(200.00m, updatedProduct.Price);
        Assert.Equal(30, updatedProduct.Count);
        _logger.LogInformation("Товар успешно обновлен с использованием только Code, Code={Code}", initialCode);

        // --- Cleanup: Удаление товара ---
        _logger.LogInformation("Удаление товара ID={ProdId}...", createdProductId);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            var deleteResult = await _apiClient!.DeleteProductAsync(new List<long> { createdProductId.Value });
            Assert.NotNull(deleteResult);
            Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        });

        Assert.Null(deleteException);
        _logger.LogInformation("Товар успешно удален, ID={ProdId}", createdProductId);
    }

    /// <summary>
    /// Интеграционный тест для проверки, сохраняются ли значения полей, которые не были переданы при обновлении товара.
    /// </summary>
    [Fact]
    public async Task UpdateProductWithNullFieldsAsync_RealApi_ShouldNotChangeValues()
    {
        if (!_canRunIntegrationTests || _apiClient == null)
        {
            _logger.LogWarning("Пропуск теста UpdateProductWithNullFieldsAsync_RealApi_ShouldNotChangeValues: Конфигурация отсутствует.");
            Assert.Fail("Пропуск интеграционного теста: Конфигурация MogutaApiIntegration не найдена или неполная.");
            return;
        }

        // --- Arrange: Создание товара ---
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var initialDescription = $"Описание товара {uniqueId}";
        var initialShortDescription = $"Краткое описание {uniqueId}";
        var initialTitle = $"Тест Товар {uniqueId}";
        var initialCode = $"TEST-CODE-{uniqueId}";
        long categoryId = 1; // Пример ID категории

        var productToCreate = new MogutaProduct
        {
            CatId = categoryId,
            Title = initialTitle,
            Code = initialCode,
            Description = initialDescription,
            ShortDescription = initialShortDescription,
            Price = 100.00m,
            Count = 10,
            Activity = true
        };

        long? createdProductId = null;
        _logger.LogInformation("Запуск теста UpdateProductWithNullFieldsAsync: Создание товара Code={Code}...", initialCode);

        var createException = await Record.ExceptionAsync(async () =>
        {
            var importResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { productToCreate });
            Assert.NotNull(importResult);
            Assert.Contains("импорт", importResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Codes = new List<string> { initialCode } };
            createdProductId = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault()?.Id;
        });

        Assert.Null(createException);
        Assert.NotNull(createdProductId);
        _logger.LogInformation("Товар создан, ID={ProdId}", createdProductId);

        // --- Act: Обновление с null для Description и ShortDescription ---
        var productToUpdate = new MogutaProduct
        {
            Id = createdProductId.Value,
            CatId = categoryId,
            Title = initialTitle, // Оставляем название без изменений
            Code = initialCode,
            Description = null, // Устанавливаем null
            ShortDescription = null, // Устанавливаем null
            Price = 150.00m, // Обновляем цену
            Count = 20, // Обновляем количество
            Activity = true
        };

        MogutaProduct? updatedProduct = null;
        var updateException = await Record.ExceptionAsync(async () =>
        {
            var updateResult = await _apiClient!.ImportProductAsync(new List<MogutaProduct> { productToUpdate });
            Assert.NotNull(updateResult);
            Assert.Contains("импорт", updateResult, StringComparison.OrdinalIgnoreCase);

            await Task.Delay(500);
            var searchParams = new GetProductRequestParams { Ids = new List<long> { createdProductId.Value } };
            updatedProduct = (await _apiClient!.GetProductAsync(searchParams))?.FirstOrDefault();
        });

        Assert.Null(updateException);
        Assert.NotNull(updatedProduct);
        Assert.Equal(initialDescription, updatedProduct!.Description); // Проверяем, что значение не изменилось
        Assert.Equal(initialShortDescription, updatedProduct.ShortDescription); // Проверяем, что значение не изменилось
        Assert.Equal(150.00m, updatedProduct.Price); // Проверяем обновленное значение
        Assert.Equal(20, updatedProduct.Count); // Проверяем обновленное значение
        _logger.LogInformation("Товар успешно обновлен, ID={ProdId}", createdProductId);

        // --- Cleanup: Удаление товара ---
        _logger.LogInformation("Удаление товара ID={ProdId}...", createdProductId);
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            var deleteResult = await _apiClient!.DeleteProductAsync(new List<long> { createdProductId.Value });
            Assert.NotNull(deleteResult);
            Assert.Contains("Удаление завершено", deleteResult, StringComparison.OrdinalIgnoreCase);
        });

        Assert.Null(deleteException);
        _logger.LogInformation("Товар успешно удален, ID={ProdId}", createdProductId);
    }

    /// <summary>
    /// Удаляем все товары
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteAllProducts_RealApi()
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
            IncludeVariants = true,
            IncludeProperties = true
        };

        // Act
        var total = 0;
        var deleteException = await Record.ExceptionAsync(async () =>
        {
            while (true)
            {
                var products = await _apiClient!.GetProductAsync(requestParams);
                if (products == null || products.Count == 0) break;
                var toDelete = products.Where(t => t.Id != null).Select(p => p.Id ?? 0).ToList();
                await _apiClient!.DeleteProductAsync(toDelete);
                total += toDelete.Count;
                Debug.WriteLine($"... удалено {toDelete.Count} товаров / всего удалено {total} товаров.");
                await Task.Delay(500);
            }
        });
        Assert.Null(deleteException);
    }


    #region Вспомогательные методы
    private MogutaProduct CreateTestProduct(long categoryId)
    {
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

        var newProduct = new MogutaProduct
        {
            // Основные данные
            CatId = categoryId,
            Title = $"Резистор тестовый {uniqueId} (10 кОм, 0.25 Вт)",
            Code = $"RES-TEST-{uniqueId}", // Артикул
            Url = $"resistor-test-{uniqueId}", // URL
            Activity = true,
            Price = 5.75m, // Цена базового варианта (или товара без вариантов)
            Count = 150, // Общее кол-во? Или кол-во базового? Зависит от логики Moguta
            Unit = "шт.",
            Weight = 0.002m, // Вес в кг?

            // Описание
            Description = $@"<p>Тестовый резистор SMD {uniqueId} для поверхностного монтажа.</p>
                             <ul>
                               <li>Сопротивление: 10 кОм</li>
                               <li>Мощность: 0.25 Вт</li>
                               <li>Точность: 5%</li>
                               <li>Типоразмер: 0805</li>
                             </ul>
                             <p>Произведен в рамках интеграционного теста.</p>",
            ShortDescription = $"Тестовый резистор {uniqueId}, 10 кОм, 5%, 0.25 Вт, SMD 0805",

            // SEO
            MetaTitle = $"Купить Тестовый Резистор {uniqueId} 10 кОм",
            MetaKeywords = $"резистор, smd, 0805, 10k, {uniqueId}, тест",
            MetaDesc = $"Интеграционный тест: резистор {uniqueId} с сопротивлением 10 кОм и мощностью 0.25 Вт.",

            // Цены и флаги
            OldPrice = 6.50m,
            Recommend = false,
            New = true,
            //Yml = true, // Выгружать в YML

            // Изображения (указываем имена файлов, если они уже есть, или полные URL для загрузки)
            ImageUrl = "https://cdn.server360.ru/global/images/b6/B6HND24101604P1QGB3.jpg|https://cdn.server360.ru/global/images/24/24-475-331-05.jpg|https://cdn.server360.ru/global/images/24/24-475-331-01.jpg",
            // ImageTitle = "Вид сверху|Схема",
            // ImageAlt = "Резистор тестовый|Схема резистора",

            // Характеристики (Property)
            // Важно: Имена характеристик ("Сопротивление", "Мощность", "Точность", "Типоразмер", "Производитель")
            // должны либо УЖЕ СУЩЕСТВОВАТЬ в Moguta, либо будут созданы с типом "string" или "textarea".
            // Если характеристика существует, тип и другие её параметры будут взяты из существующей.
            Property = new List<MogutaProperty>
            {
                new MogutaProperty { Name = "Сопротивление", Value = "10 кОм", Type = "string"}, // Если хар-ка есть, тип можно не указывать
                new MogutaProperty { Name = "Мощность", Value = "0.25 Вт", Type = "string"},
                new MogutaProperty { Name = "Accuracy (%)", Value = "5", Type = "string"}, // Число как строка
                new MogutaProperty { Name = "SizeType", Value = "0805", Type = "string"},
                new MogutaProperty { Name = "Manufacturer-1", Value = $"TestCorp-{uniqueId}", Type = "string"},
                new MogutaProperty { Name = "Комментарий", Value = $"Создано тестом {DateTime.Now}", Type = "textarea"}
            },

            // Варианты (Variants)
            // Артикулы вариантов должны быть уникальны!
            // Цены, кол-во и вес вариантов могут переопределять базовые значения товара.
            Variants = new List<MogutaVariant>
            {
                new MogutaVariant {
                    TitleVariant = "-Var1", // Добавка к основному названию
                    Code = $"RES-TEST-{uniqueId}", // Уникальный артикул варианта
                    Price = 5.75m, // Другая цена
                    OldPrice = 8,
                    Activity = true,
                    Weight = 0.002m
                },
                // Вариант с точностью 1%
                new MogutaVariant {
                    TitleVariant = "-Var2", // Добавка к основному названию
                    Code = $"RES-TEST-{uniqueId}-2", // Уникальный артикул варианта
                    Price = 7, // Другая цена
                    OldPrice = 8,
                    Count = 50,    // Другое количество
                    Activity = true,
                    Weight = 3, // Чуть другой вес
                },
                 // Вариант с другой мощностью (если это возможно как вариант, а не отдельный товар)
                 new MogutaVariant {
                    TitleVariant = "-Var3",
                    Code = $"RES-TEST-{uniqueId}-3",
                    Price = 8.20m,
                    Count = 100,
                    Activity = true,
                    Weight = 0.003m,
                },
                 // Неактивный вариант
                 new MogutaVariant {
                    TitleVariant = "Снято с производства",
                    Code = $"RES-TEST-{uniqueId}-4",
                    Price = 4.00m,
                    Count = 0,
                    Activity = false, // Неактивен
                    Weight = 0.002m
                }
            }
        };

        return newProduct;
    }

    #endregion

}