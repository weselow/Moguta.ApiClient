# Moguta.ApiClient .NET

Библиотека C#/.NET для взаимодействия с API MogutaCMS.

## Возможности

*   Поддержка основных методов API MogutaCMS (Товары, Категории, Заказы, Пользователи, Тест, Доп. поля).
*   Асинхронные методы (`async`/`await`).
*   Использование `HttpClient` и `System.Text.Json`.
*   Автоматическая проверка подписи ответа API (можно отключить).
*   Строгая типизация запросов и ответов (DTO).
*   Обработка ошибок API и сети с использованием кастомных исключений.
*   Поддержка Dependency Injection (DI) для легкой интеграции (например, в ASP.NET Core).
*   Логирование с использованием `Microsoft.Extensions.Logging` (интеграция с NLog, Serilog и т.д.).
*   Русскоязычная XML-документация.

## Установка

Добавьте ссылку на проект или установите NuGet пакет (если будет опубликован).

```bash
# dotnet add package Moguta.ApiClient --version <version>
```

Также убедитесь, что установлены необходимые зависимости для логирования и конфигурации (пример для NLog и ASP.NET Core):

```bash
# dotnet add package NLog.Web.AspNetCore
```

## Конфигурация

Добавьте секцию в ваш файл конфигурации (`appsettings.json` или аналог):

```json
{
  "MogutaApi": {
    "SiteUrl": "https://your-moguta-site.ru", // URL вашего сайта без /api
    "Token": "YOUR_API_TOKEN",                // Токен из админки MogutaCMS
    "SecretKey": "YOUR_SECRET_KEY",           // Секретный ключ из админки MogutaCMS
    "ValidateApiResponseSignature": true,     // Рекомендуется оставить true
    "RequestTimeout": "00:01:30"              // Опционально: таймаут запроса (ЧЧ:ММ:СС)
  },
  // ... другие настройки ...
  "Logging": {
      // Настройки логирования
  }
}
```

Настройте провайдер логирования (например, NLog), добавив `nlog.config` (пример есть в репозитории) и вызвав `builder.Host.UseNLog()` в `Program.cs`.

## Регистрация в Dependency Injection (ASP.NET Core)

В `Program.cs` (или `Startup.cs`):

```csharp
using Moguta.ApiClient.Extensions;
using NLog.Web; // Если используете NLog

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования (пример для NLog)
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Регистрация клиента Moguta API
// Считывает настройки из секции "MogutaApi"
builder.Services.AddMogutaApiClient("MogutaApi");

// Добавление других сервисов
builder.Services.AddControllersWithViews();
// ...

var app = builder.Build();

// Настройка конвейера HTTP-запросов
// ...

app.Run();
```

## Использование Клиента

Внедрите интерфейс `IMogutaApiClient` в ваш сервис или контроллер через конструктор:

```csharp
using Microsoft.AspNetCore.Mvc;
using Moguta.ApiClient.Abstractions;
using Moguta.ApiClient.Exceptions;
using Moguta.ApiClient.Models.Common;
using Moguta.ApiClient.Models.Requests;
using Microsoft.Extensions.Logging;

public class ShopIntegrationService
{
    private readonly IMogutaApiClient _mogutaApi;
    private readonly ILogger<ShopIntegrationService> _logger;

    public ShopIntegrationService(IMogutaApiClient mogutaApi, ILogger<ShopIntegrationService> logger)
    {
        _mogutaApi = mogutaApi;
        _logger = logger;
    }

    // Пример: Получение списка категорий постранично
    public async Task<List<Category>?> GetCategoriesPage(int page = 1, int count = 20)
    {
        _logger.LogInformation("Запрос категорий: страница {Page}, количество {Count}", page, count);
        try
        {
            var requestParams = new GetCategoryRequestParams { Page = page, Count = count };
            var categories = await _mogutaApi.GetCategoryAsync(requestParams);
            _logger.LogInformation("Получено {Count} категорий.", categories?.Count ?? 0);
            return categories;
        }
        catch (MogutaApiException ex)
        {
            _logger.LogError(ex, "Ошибка API при получении категорий. Код={Code}, Сообщение='{Msg}'", ex.ApiErrorCode, ex.ApiErrorMessage);
            // Обработка ошибки (например, возврат null или пустого списка)
            return null;
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Неожиданная ошибка при получении категорий.");
            throw; // Перевыбросить для обработки выше
        }
    }

    // Пример: Обновление остатка конкретного товара
    public async Task<bool> UpdateProductStock(long productId, decimal newStock)
    {
        _logger.LogInformation("Обновление остатка для товара ID {ProductId} на {NewStock}", productId, newStock);
        try
        {
            // Создаем объект товара только с необходимыми для обновления полями (ID и остаток)
            var productUpdate = new Product
            {
                Id = productId,
                Count = newStock
                // Можно добавить ID склада, если нужно обновить на конкретном складе:
                // Storage = "your_storage_id"
            };

            string? result = await _mogutaApi.ImportProductAsync(new List<Product> { productUpdate });

            _logger.LogInformation("Результат обновления остатка товара ID {ProductId}: {Result}", productId, result);
            // Проверяем успешность по ответу API (текст может отличаться)
            return result?.Contains("Обновлено: 1", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Ошибка при обновлении остатка товара ID {ProductId}", productId);
            return false;
        }
    }

     // Пример: Поиск пользователя по Email
     public async Task<User?> FindUserByEmail(string email)
     {
         _logger.LogInformation("Поиск пользователя с email {Email}", email);
         try
         {
             User? user = await _mogutaApi.FindUserAsync(email);
             if (user != null)
             {
                 _logger.LogInformation("Найден пользователь: ID={UserId}, Имя={UserName}", user.Id, user.Name);
             }
             else
             {
                 _logger.LogInformation("Пользователь с email {Email} не найден.", email);
             }
             return user;
         }
        catch (Exception ex)
         {
              _logger.LogError(ex, "Ошибка при поиске пользователя с email {Email}", email);
             return null;
         }
     }
}
```

## Обработка `order_content`

Поле `order_content` в заказах представляет особую сложность, так как Moguta API исторически использует PHP `serialize()` для хранения позиций заказа.

*   **При получении заказов (`GetOrderAsync`):**
    *   Клиент пытается десериализовать `order_content` как JSON (на случай, если заказ был создан через этот же клиент).
    *   Если десериализация JSON успешна, позиции заказа будут доступны в свойстве `Order.OrderItems`, а `Order.OrderContent` будет `null`.
    *   Если `order_content` не является валидным JSON (вероятно, PHP строка), он остается в `Order.OrderContent` как `string?`, а `Order.OrderItems` будет `null`. Автоматическая десериализация PHP строк не поддерживается.
*   **При импорте заказов (`ImportOrderAsync`):**
    *   Вы должны заполнить свойство `Order.OrderItems` списком объектов `OrderItem`.
    *   Клиент **автоматически** сериализует `Order.OrderItems` в **JSON строку** и отправит ее в API в поле `param` -> `orders` -> `order_content`.
    *   **Внимание:** Успешность этого подхода **зависит от способности API сервера MogutaCMS** принять и обработать JSON строку в поле `order_content`. Это необходимо проверить на вашем экземпляре MogutaCMS. Если API ожидает исключительно PHP `serialize()` строку, создание/обновление позиций заказа через этот клиент будет невозможно без модификации API.

## Лицензия
