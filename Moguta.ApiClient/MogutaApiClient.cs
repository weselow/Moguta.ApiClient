using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moguta.ApiClient.Abstractions;
using Moguta.ApiClient.Exceptions;
using Moguta.ApiClient.Infrastructure;
using Moguta.ApiClient.Models.Common;
using Moguta.ApiClient.Models.Requests;
using Moguta.ApiClient.Models.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json; // Для JsonException и JsonElement

namespace Moguta.ApiClient;

/// <summary>
/// Клиент для взаимодействия с MogutaCMS API. Реализует <see cref="IMogutaApiClient"/>.
/// </summary>
/// <remarks>
/// Этот клиент использует HttpClient для выполнения запросов и System.Text.Json для сериализации/десериализации.
/// Он обрабатывает форматирование запросов, парсинг ответов, проверку подписи и обработку ошибок.
/// Используйте методы расширения <see cref="Moguta.ApiClient.Extensions.ServiceCollectionExtensions"/> для легкой регистрации в DI контейнерах.
/// </remarks>
public partial class MogutaApiClient : IMogutaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly MogutaApiClientOptions _options;
    private readonly ILogger<MogutaApiClient> _logger;
    private const string ApiPath = "/api"; // Относительный путь к API

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MogutaApiClient"/>.
    /// </summary>
    /// <param name="httpClient">Экземпляр HttpClient.</param>
    /// <param name="options">Опции конфигурации.</param>
    /// <param name="logger">Экземпляр логгера.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если httpClient, options или logger равны null.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если опции невалидны (например, отсутствует SiteUrl, Token или SecretKey).</exception>
    public MogutaApiClient(
        HttpClient httpClient,
        IOptions<MogutaApiClientOptions> options, // Используем IOptions для поддержки обновлений конфигурации
        ILogger<MogutaApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options)); // Получаем значение из IOptions
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Простая валидация опций при создании клиента
        if (string.IsNullOrWhiteSpace(_options.SiteUrl) || !Uri.TryCreate(_options.SiteUrl, UriKind.Absolute, out _))
            throw new ArgumentException("SiteUrl обязателен и должен быть валидным абсолютным URL.", $"{nameof(options)}.{nameof(_options.SiteUrl)}");
        if (string.IsNullOrWhiteSpace(_options.Token))
            throw new ArgumentException("Token обязателен.", $"{nameof(options)}.{nameof(_options.Token)}");
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new ArgumentException("SecretKey обязателен.", $"{nameof(options)}.{nameof(_options.SecretKey)}");

        // Конфигурация HttpClient
        try
        {
            _httpClient.BaseAddress = new Uri(_options.SiteUrl.TrimEnd('/') + ApiPath + "/"); // Убеждаемся в наличии слеша в конце
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException($"Неверный формат SiteUrl: '{_options.SiteUrl}'. Ошибка: {ex.Message}", $"{nameof(options)}.{nameof(_options.SiteUrl)}", ex);
        }

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // Добавляем User-Agent для идентификации клиента
        var assemblyVersion = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Moguta.ApiClient.NET/{assemblyVersion}");

        if (_options.RequestTimeout.HasValue)
        {
            _httpClient.Timeout = _options.RequestTimeout.Value;
        }

        _logger.LogInformation("MogutaApiClient инициализирован. BaseAddress: {BaseAddress}, ValidateSignature: {ValidateSignature}", _httpClient.BaseAddress, _options.ValidateApiResponseSignature);
    }

    // --- Приватный Вспомогательный Метод для Отправки Запросов ---

    /// <summary>
    /// Отправляет запрос к Moguta API.
    /// </summary>
    /// <param name="expectedPayloadType">Ожидаемый тип данных в поле 'response'.</param>
    /// <param name="apiMethod">Имя API метода (например, "getProduct").</param>
    /// <param name="parameters">Объект с параметрами запроса (будет сериализован в JSON).</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Десериализованные данные из поля 'response' ответа API как object?.</returns>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках уровня API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    /// <exception cref="HttpRequestException">Выбрасывается при базовых сетевых ошибках.</exception>
    /// <exception cref="JsonException">Выбрасывается при ошибках сериализации/десериализации JSON.</exception>
    /// <exception cref="ArgumentNullException">Выбрасывается, если apiMethod пуст.</exception>
    private async Task<object?> SendApiRequestAsync(
        Type expectedPayloadType,
        string apiMethod,
        object? parameters,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiMethod))
        {
            throw new ArgumentNullException(nameof(apiMethod));
        }

        string parametersJson = parameters == null ? "{}" : SerializationHelper.Serialize(parameters);
        var requestData = new Dictionary<string, string>
        {
            { "token", _options.Token },
            { "method", apiMethod },
            { "param", parametersJson }
        };

        using var content = new FormUrlEncodedContent(requestData);

        // --- Логирование ---
        string actualRequestBody = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Фактическое тело запроса (FormUrlEncoded) для метода {ApiMethod}: {RequestBody}", apiMethod, actualRequestBody);
        _logger.LogInformation("Отправка запроса к API. Метод: {ApiMethod}, Endpoint: {Endpoint}", apiMethod, _httpClient.BaseAddress);
        _logger.LogDebug("Параметры запроса (JSON): {ParametersJson}", parametersJson);
        // --- Конец логирования ---


        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("", content, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP запроса. Метод: {ApiMethod}. Ошибка: {ErrorMessage}", apiMethod, ex.Message);
            throw new MogutaApiException($"Ошибка HTTP запроса для метода '{apiMethod}'. См. внутреннее исключение.", apiMethod, null, null, ex);
        }
        catch (TaskCanceledException ex)
        {
            bool isTimeout = ex.InnerException is TimeoutException || !cancellationToken.IsCancellationRequested;
            string reason = isTimeout ? $"таймаут ({_httpClient.Timeout.TotalMilliseconds}ms)" : "операция отменена";
            _logger.LogError(ex, "Запрос к API прерван ({Reason}). Метод: {ApiMethod}.", reason, apiMethod);
            throw new MogutaApiException($"Запрос к API прерван ({reason}) для метода '{apiMethod}'.", apiMethod, null, null, ex);
        }

        //todo получение ответа
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Получен ответ от API. Метод: {ApiMethod}, Status Code: {StatusCode}", apiMethod, response.StatusCode);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            string bodySnippet = responseBody.Length > 1000 ? responseBody.Substring(0, 1000) + "..." : responseBody;
            _logger.LogDebug("Тело ответа: {ResponseBodySnippet}", bodySnippet);
        }

        MogutaApiException? pendingException = null;

        // 1. Обработка неуспешных HTTP статус кодов
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Запрос к API завершился с ошибкой HTTP {StatusCode}. Метод: {ApiMethod}. Ответ: {ResponseBody}",
                             response.StatusCode, apiMethod, responseBody);
            string apiErrorMsg = responseBody;
            string? apiErrorCode = null;
            try
            {
                var errorResponse = SerializationHelper.Deserialize<MogutaApiResponse<JsonElement>>(responseBody);
                if (errorResponse != null)
                {
                    apiErrorMsg = errorResponse.Message ?? errorResponse.Response.ToString() ?? responseBody;
                    apiErrorCode = errorResponse.Error;
                }
            }
            catch { /* Игнорируем */ }
            pendingException = new MogutaApiException($"Запрос к API завершился ошибкой HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) для метода '{apiMethod}'.",
                                             apiMethod, apiErrorCode, apiErrorMsg);
        }

        // 2. Десериализация базового ответа
        MogutaApiResponse<JsonElement>? baseApiResponse = null;
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            _logger.LogError("API вернул статус код {StatusCode}, но пустое тело ответа. Метод: {ApiMethod}", response.StatusCode, apiMethod);
            pendingException ??= new MogutaApiException($"API вернул статус код {response.StatusCode}, но пустое тело ответа.", apiMethod);
        }
        else
        {
            try
            {
                baseApiResponse = SerializationHelper.Deserialize<MogutaApiResponse<JsonElement>>(responseBody);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Критическая ошибка: не удалось десериализовать базовую структуру ответа API. Метод: {ApiMethod}. Тело: {ResponseBody}", apiMethod, responseBody.Length > 500 ? responseBody.Substring(0, 500) + "..." : responseBody);
                throw pendingException ?? new MogutaApiException($"Критическая ошибка: не удалось десериализовать ответ API для метода '{apiMethod}'. См. внутреннее исключение.", apiMethod, null, null, ex);
            }
        }

        if (baseApiResponse == null)
        {
            throw pendingException ?? new MogutaApiException($"Не удалось получить структуру ответа API для метода '{apiMethod}'.", apiMethod);
        }

        // 3. Проверка подписи
        if (_options.ValidateApiResponseSignature && baseApiResponse.Sign != null)
        {
            bool isSignatureValid = SignatureHelper.ValidateApiResponseSignature(
                baseApiResponse.Sign, _options.Token, apiMethod, parametersJson, _options.SecretKey, _logger);
            if (!isSignatureValid)
            {
                throw new MogutaApiSignatureException(
                    "Проверка подписи ответа API не удалась.",
                    baseApiResponse.Sign, "[Calculated]", apiMethod);
            }
        }

        // 4. Проверка статуса API
        if (!string.Equals(baseApiResponse.Status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            string errorMessage = baseApiResponse.Message
                                 ?? baseApiResponse.Response.ToString()
                                 ?? $"API вернул статус '{baseApiResponse.Status}' без дополнительного сообщения.";
            _logger.LogError("API вернул статус не 'OK'. Статус: {ApiStatus}, Код ошибки: {ErrorCode}, Сообщение: {ErrorMessage}, Метод: {ApiMethod}",
                             baseApiResponse.Status, baseApiResponse.Error ?? "N/A", errorMessage, apiMethod);

            if (pendingException != null)
            {
                throw new MogutaApiException(pendingException.Message + $" Детали API: Статус={baseApiResponse.Status}, Код={baseApiResponse.Error ?? "N/A"}, Сообщение={errorMessage}",
                                             apiMethod, baseApiResponse.Error, errorMessage, pendingException.InnerException);
            }
            else
            {
                throw new MogutaApiException($"API вернул статус не 'OK' для метода '{apiMethod}'. Статус: {baseApiResponse.Status}, Код ошибки: [{baseApiResponse.Error ?? "N/A"}], Сообщение: {errorMessage}",
                                         apiMethod, baseApiResponse.Error, errorMessage);
            }
        }

        // Если была ошибка HTTP, но дошли сюда - выбрасываем HTTP ошибку
        if (pendingException != null)
        {
            throw pendingException;
        }

        // 5. Десериализация payload
        object? resultPayload = null;
        try
        {
            if (baseApiResponse.Status == "OK" && baseApiResponse.Response.ValueKind != JsonValueKind.Null && baseApiResponse.Response.ValueKind != JsonValueKind.Undefined)
            {
                resultPayload = baseApiResponse.Response.Deserialize(expectedPayloadType, SerializationHelper.DefaultJsonSerializerOptions);
                _logger.LogDebug("Успешно десериализовано поле 'response' как {TargetType}. Метод: {ApiMethod}", expectedPayloadType.FullName, apiMethod);
            }
            else if (baseApiResponse.Status == "OK")
            {
                _logger.LogWarning("API вернул статус 'OK', но поле 'response' было null, undefined или не соответствовало типу {TargetType}. Метод: {ApiMethod}. Response JsonElement: {ResponseJson}",
                                 expectedPayloadType.FullName, apiMethod, baseApiResponse.Response.ToString());
                resultPayload = default; // Уже null, но для ясности
            }
        }
        catch (JsonException ex) // Ловим ошибки финальной десериализации payload при статусе OK
        {
            // Проверяем, был ли response строкой (что часто означает "не найдено")
            if (baseApiResponse.Response.ValueKind == JsonValueKind.String)
            {
                string responseString = baseApiResponse.Response.GetString() ?? "";
                _logger.LogWarning(ex, "Не удалось десериализовать поле 'response' (которое было строкой: '{ResponseStr}') в тип {TargetType}, хотя статус API был 'OK'. Метод: {ApiMethod}. Возвращаем default.",
                    responseString, expectedPayloadType.FullName, apiMethod);
                // Возвращаем default (null для object?), т.к. это может быть ожидаемым "не найдено"
                resultPayload = default;
            }
            else // Если response был объектом или массивом, но структура не подошла - это ошибка
            {
                _logger.LogError(ex, "Ошибка при десериализации поля 'response' в тип {TargetType}, хотя статус API был 'OK'. Метод: {ApiMethod}. Response JsonElement: {ResponseJson}",
                    expectedPayloadType.FullName, apiMethod, baseApiResponse.Response.ToString());
                // Выбрасываем исключение
                throw new MogutaApiException($"API вернул статус 'OK', но не удалось преобразовать поле 'response' в ожидаемый тип {expectedPayloadType.Name} для метода '{apiMethod}'. См. внутреннее исключение.",
                    apiMethod, baseApiResponse.Error, baseApiResponse.Response.ToString(), ex);
            }
        }
        catch (Exception ex) // Другие неожиданные ошибки
        {
            _logger.LogError(ex, "Неожиданная ошибка при финальной десериализации 'response'. Метод: {ApiMethod}.", apiMethod);
            throw new MogutaApiException($"Неожиданная ошибка при обработке ответа API для метода '{apiMethod}'. См. внутреннее исключение.", apiMethod, null, null, ex);
        }

        // Если мы дошли сюда без исключений и статус API был "OK"
        if (baseApiResponse.Status == "OK")
        {
            _logger.LogInformation("Запрос к API успешно выполнен. Метод: {ApiMethod}", apiMethod);
        }
        // Если изначально была ошибка HTTP или API статус не OK, она будет выброшена выше

        return resultPayload; // Возвращаем десериализованный payload или null/default
    }


    // --- Реализация Публичных Методов API ---

    #region Методы Товаров (Product)

    /// <inheritdoc />
    public async Task<string?> ImportProductAsync(List<Product> products, CancellationToken cancellationToken = default)
    {
        if (products == null || products.Count == 0)
            throw new ArgumentException("Список товаров не может быть null или пустым.", nameof(products));
        if (products.Count > 100)
            _logger.LogWarning("Импорт товаров: количество {ProductCount} превышает рекомендуемый лимит в 100.", products.Count);

        _logger.LogInformation("Попытка импорта {ProductCount} товаров.", products.Count);
        var parameters = new ImportProductRequestParams { Products = products };
        var response = await SendApiRequestAsync(typeof(string), "importProduct", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат импорта товаров: {Result}", result);
        return result;
    }

    /// <inheritdoc />
    public async Task<string?> DeleteProductAsync(List<long> productIds, CancellationToken cancellationToken = default)
    {
        if (productIds == null || productIds.Count == 0)
            throw new ArgumentException("Список ID товаров не может быть null или пустым.", nameof(productIds));

        _logger.LogInformation("Попытка удаления {ProductCount} товаров с ID: {ProductIds}", productIds.Count, string.Join(",", productIds));
        var parameters = new DeleteProductRequestParams { ProductIds = productIds };
        var response = await SendApiRequestAsync(typeof(string), "deleteProduct", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат удаления товаров: {Result}", result);
        return result;
    }

    /// <inheritdoc />
    public async Task<List<Product>?> GetProductAsync(GetProductRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка получения товаров с параметрами: {@RequestParams}", requestParams);
        var response = await SendApiRequestAsync(typeof(GetProductResponsePayload), "getProduct", requestParams, cancellationToken).ConfigureAwait(false);
        var payload = response as GetProductResponsePayload;
        var products = payload?.Products ?? new List<Product>(); // <-- Возвращаем пустой список
        _logger.LogInformation("Успешно получено {ProductCount} товаров (TotalCount={TotalCount}).", products.Count, payload?.TotalCount ?? 0);
        return products;
    }

    #endregion

    #region Методы Категорий (Category)
    /// <inheritdoc />
    public async Task<List<Category>?> GetCategoryAsync(GetCategoryRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка получения категорий с параметрами: {@RequestParams}", requestParams);
        var response = await SendApiRequestAsync(typeof(GetCategoryResponsePayload), "getCategory", requestParams, cancellationToken).ConfigureAwait(false);
        var payload = response as GetCategoryResponsePayload;

        // Если payload == null (т.к. SendApiRequestAsync вернул null из-за строки "не найдено" или реального null/undefined),
        // то возвращаем ПУСТОЙ СПИСОК, а не null, т.к. метод ожидает список.
        var categories = payload?.Categories ?? new List<Category>(); // <-- ИЗМЕНЕНИЕ: Возвращаем пустой список, если payload null

        _logger.LogInformation("Успешно получено {CategoryCount} категорий (TotalCount={TotalCount}).", categories.Count, payload?.TotalCount ?? 0);
        return categories;
    }

    /// <inheritdoc />
    public async Task<string?> ImportCategoryAsync(List<Category> categories, CancellationToken cancellationToken = default)
    {
        if (categories == null || categories.Count == 0)
            throw new ArgumentException("Список категорий не может быть null или пустым.", nameof(categories));
        if (categories.Count > 100)
            _logger.LogWarning("Импорт категорий: количество {CategoryCount} превышает рекомендуемый лимит в 100.", categories.Count);

        _logger.LogInformation("Попытка импорта {CategoryCount} категорий.", categories.Count);
        var parameters = new ImportCategoryRequestParams { Categories = categories };
        var response = await SendApiRequestAsync(typeof(string), "importCategory", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат импорта категорий: {Result}", result);
        return result;
    }

    /// <inheritdoc />
    public async Task<string?> DeleteCategoryAsync(List<long> categoryIds, CancellationToken cancellationToken = default)
    {
        if (categoryIds == null || categoryIds.Count == 0)
            throw new ArgumentException("Список ID категорий не может быть null или пустым.", nameof(categoryIds));

        _logger.LogInformation("Попытка удаления {CategoryCount} категорий с ID: {CategoryIds}", categoryIds.Count, string.Join(",", categoryIds));
        var parameters = new DeleteCategoryRequestParams { CategoryIds = categoryIds };
        var response = await SendApiRequestAsync(typeof(string), "deleteCategory", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат удаления категорий: {Result}", result);
        return result;
    }
    #endregion

    #region Методы Заказов (Order)
    /// <inheritdoc />
    public async Task<List<Order>?> GetOrderAsync(GetOrderRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка получения заказов с параметрами: {@RequestParams}", requestParams);
        var response = await SendApiRequestAsync(typeof(GetOrderResponsePayload), "getOrder", requestParams, cancellationToken).ConfigureAwait(false);
        var payload = response as GetOrderResponsePayload;
        var orders = payload?.Orders ?? new List<Order>(); // <-- Возвращаем пустой список

        // Постобработка OrderContent остается
        if (orders.Count > 0)
        {
            foreach (var order in orders)
            {
                if (order.OrderItems != null) // Проверяем, десериализовал ли JsonPropertyName
                {
                    _logger.LogDebug("Заказ ID {OrderId}: OrderItems count = {Count} (десериализовано из JSON)", order.Id, order.OrderItems.Count);
                    order.OrderContent = null;
                }
                else if (!string.IsNullOrWhiteSpace(order.OrderContent)) // Если OrderItems null, но есть строка OrderContent
                {
                    string content = order.OrderContent.Trim();
                    if (content.StartsWith("a:") && content.Contains("{") && content.EndsWith("}")) // Похоже на PHP serialize
                    {
                        string contentSnippet = content.Length > 100 ? content.Substring(0, 100) + "..." : content;
                        _logger.LogWarning("OrderContent для заказа ID {OrderId} является строкой PHP serialize. Автоматическая десериализация невозможна. Контент (начало): {OrderContentSnippet}", order.Id, contentSnippet);
                    }
                    else // Не JSON и не PHP serialize?
                    {
                        _logger.LogWarning("OrderContent для заказа ID {OrderId} содержит неожиданную строку: {Content}", order.Id, order.OrderContent);
                    }
                    // OrderItems остается null
                }
            }
        }

        _logger.LogInformation("Успешно получено {OrderCount} заказов (TotalCount={TotalCount}).", orders.Count, payload?.TotalCount ?? 0);
        return orders;
    }

    /// <inheritdoc />
    public async Task<string?> ImportOrderAsync(List<Order> orders, CancellationToken cancellationToken = default)
    {
        if (orders == null || orders.Count == 0)
            throw new ArgumentException("Список заказов не может быть null или пустым.", nameof(orders));
        if (orders.Count > 100)
            _logger.LogWarning("Импорт заказов: количество {OrderCount} превышает рекомендуемый лимит в 100.", orders.Count);

        _logger.LogInformation("Попытка импорта {OrderCount} заказов.", orders.Count);

        // Сериализуем OrderItems в JSON и помещаем в OrderContent
        foreach (var order in orders)
        {
            if (order.OrderItems != null && order.OrderItems.Count > 0)
            {
                try
                {
                    order.OrderContent = SerializationHelper.Serialize(order.OrderItems);
                    string contentSnippet = order.OrderContent.Length > 200 ? order.OrderContent.Substring(0, 200) + "..." : order.OrderContent;
                    _logger.LogDebug("Сериализованы OrderItems в JSON для заказа ID {OrderId} (если есть): {JsonContentSnippet}", order.Id, contentSnippet);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Не удалось сериализовать OrderItems в JSON для заказа ID {OrderId}.", order.Id);
                    throw new MogutaApiException($"Не удалось сериализовать OrderItems для заказа ID {order.Id}.", "importOrder", null, null, ex);
                }
            }
            else
            {
                if (order.OrderItems == null && !string.IsNullOrEmpty(order.OrderContent))
                {
                    _logger.LogWarning("Заказ ID {OrderId} имеет заданный вручную OrderContent, но нет OrderItems. Отправляется существующий OrderContent.", order.Id);
                }
                else
                {
                    order.OrderContent = null;
                }
            }
        }

        var parameters = new ImportOrderRequestParams { Orders = orders };
        var response = await SendApiRequestAsync(typeof(string), "importOrder", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат импорта заказов: {Result}", result);
        return result;
    }

    /// <inheritdoc />
    public async Task<string?> DeleteOrderAsync(List<long> orderIds, CancellationToken cancellationToken = default)
    {
        if (orderIds == null || orderIds.Count == 0)
            throw new ArgumentException("Список ID заказов не может быть null или пустым.", nameof(orderIds));

        _logger.LogInformation("Попытка удаления {OrderCount} заказов с ID: {OrderIds}", orderIds.Count, string.Join(",", orderIds));
        var parameters = new DeleteOrderRequestParams { OrderIds = orderIds };
        var response = await SendApiRequestAsync(typeof(string), "deleteOrder", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат удаления заказов: {Result}", result);
        return result;
    }
    #endregion

    #region Методы Пользователей (User)
    /// <inheritdoc />
    public async Task<List<User>?> GetUserAsync(GetUserRequestParams requestParams, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка получения пользователей с параметрами: {@RequestParams}", requestParams);
        var response = await SendApiRequestAsync(typeof(GetUserResponsePayload), "getUsers", requestParams, cancellationToken).ConfigureAwait(false);
        var payload = response as GetUserResponsePayload;
        var users = payload?.Users ?? new List<User>(); // <-- Возвращаем пустой список
        _logger.LogInformation("Успешно получено {UserCount} пользователей (TotalCount={TotalCount}).", users.Count, payload?.TotalCount ?? 0);
        return users;
    }

    /// <inheritdoc />
    public async Task<string?> ImportUserAsync(List<User> users, bool? enableUpdate = true, CancellationToken cancellationToken = default)
    {
        if (users == null || users.Count == 0)
            throw new ArgumentException("Список пользователей не может быть null или пустым.", nameof(users));
        if (users.Count > 100)
            _logger.LogWarning("Импорт пользователей: количество {UserCount} может превышать рекомендуемый лимит.", users.Count);

        _logger.LogInformation("Попытка импорта {UserCount} пользователей. EnableUpdate={EnableUpdate}", users.Count, enableUpdate);
        var parameters = new ImportUserRequestParams { Users = users, EnableUpdate = enableUpdate };
        var response = await SendApiRequestAsync(typeof(string), "importUsers", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат импорта пользователей: {Result}", result);
        return result;
    }

    /// <inheritdoc />
    public async Task<string?> DeleteUserAsync(List<string> emails, CancellationToken cancellationToken = default)
    {
        if (emails == null || emails.Count == 0)
            throw new ArgumentException("Список email не может быть null или пустым.", nameof(emails));

        _logger.LogInformation("Попытка удаления {UserCount} пользователей с email: {Emails}", emails.Count, string.Join(", ", emails));
        var parameters = new DeleteUserRequestParams { Emails = emails };
        var response = await SendApiRequestAsync(typeof(string), "deleteUser", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат удаления пользователей: {Result}", result);
        return result;
    }

    /// <summary>
    /// Находит одного пользователя в MogutaCMS по его email адресу.
    /// </summary>
    /// <param name="email">Email адрес пользователя для поиска.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит найденный объект <see cref="User"/> или <c>null</c>, если пользователь с таким email не существует.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если email null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API (кроме случая, когда API возвращает строку "не найден") или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    public async Task<User?> FindUserAsync(string email, CancellationToken cancellationToken = default)
    {
        // Проверка входного параметра
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть null или пустым.", nameof(email));

        _logger.LogInformation("Попытка поиска пользователя с email: {Email}", email);
        var parameters = new FindUserRequestParams { Email = email };

        try
        {
            // Вызываем базовый метод отправки запроса, ожидая получить объект типа User.
            // SendApiRequestAsync вернет User?, если API вернет объект User или null в поле response.
            // SendApiRequestAsync выбросит MogutaApiException с InnerException = JsonException,
            // если API вернет status:OK, но response будет строкой (например, "не найден") или другим не-User объектом.
            var response = await SendApiRequestAsync(typeof(User), "findUser", parameters, cancellationToken).ConfigureAwait(false);
            var user = response as User; // Безопасное приведение к User?

            if (user != null)
            {
                // Пользователь успешно найден и десериализован
                _logger.LogInformation("Успешно найден пользователь с email: {Email}, UserID: {UserId}", email, user.Id);
            }
            else
            {
                // SendApiRequestAsync вернул null. Это могло случиться, если API вернуло response: null.
                _logger.LogInformation("Пользователь с email {Email} не найден (API вернул null/undefined в response при статусе OK).", email);
            }
            return user; // Возвращаем найденного пользователя или null
        }
        // Перехватываем специфичное исключение, которое SendApiRequestAsync выбрасывает,
        // когда статус ответа API = "OK", но поле "response" не удалось десериализовать в User.
        catch (MogutaApiException ex) when (ex.InnerException is JsonException jsonEx)
        {
            // Проверяем, содержит ли исходное сообщение от API (которое теперь в ApiErrorMessage)
            // текст, указывающий на то, что пользователь не найден.
            bool isNotFoundMessage = ex.ApiErrorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) ??
                                    ex.ApiErrorMessage?.Contains("не найден", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isNotFoundMessage)
            {
                // Если это ожидаемое сообщение "не найден", то это не ошибка, а нормальный результат поиска.
                // Логируем это и возвращаем null.
                _logger.LogInformation("Пользователь с email {Email} не найден (API вернул строку в response: '{ApiErrorMsg}'). Исключение обработано.", email, ex.ApiErrorMessage);
                return null;
            }

            // Если статус был OK, но произошла ДРУГАЯ ошибка десериализации
            // (например, неверный формат поля 'role' или 'last_updated' у НАЙДЕННОГО пользователя),
            // то это реальная проблема с DTO или ответом API. Пробрасываем исключение дальше.
            _logger.LogError(ex, "Неожиданная ошибка десериализации при поиске пользователя {Email}, но сообщение не похоже на 'не найден'. Возможно, некорректен DTO User или ответ API для существующего пользователя.", email);
            throw; // Перевыбросить исходное исключение MogutaApiException с JsonException внутри
        }
        // Любые другие исключения (MogutaApiException с другим InnerException, MogutaApiSignatureException, HttpRequestException и т.д.)
        // будут проброшены из SendApiRequestAsync без перехвата здесь, сигнализируя о более серьезных проблемах.
    }
    
    #endregion

    #region Служебные Методы
    /// <inheritdoc />
    public async Task<TestResponsePayload?> TestConnectionAsync(object parameters, CancellationToken cancellationToken = default)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        _logger.LogInformation("Выполнение тестового подключения к API с параметрами: {@Parameters}", parameters);
        // Ожидаем словарь Dictionary<string, object>, который является TestResponsePayload
        var response = await SendApiRequestAsync(typeof(TestResponsePayload), "test", parameters, cancellationToken).ConfigureAwait(false);
        var resultPayload = response as TestResponsePayload;
        _logger.LogInformation("Тестовое подключение к API успешно. Получен ответ: {@Response}", resultPayload);
        return resultPayload;
    }

    /// <inheritdoc />
    public async Task<string?> CreateOrUpdateOrderCustomFieldsAsync(List<CustomFieldDefinition> fieldDefinitions, CancellationToken cancellationToken = default)
    {
        if (fieldDefinitions == null || fieldDefinitions.Count == 0)
            throw new ArgumentException("Список определений полей не может быть null или пустым.", nameof(fieldDefinitions));

        _logger.LogInformation("Попытка создания/обновления {FieldCount} дополнительных полей заказа.", fieldDefinitions.Count);

        foreach (var field in fieldDefinitions)
        {
            if (string.IsNullOrWhiteSpace(field.Name) || string.IsNullOrWhiteSpace(field.Type))
                throw new ArgumentException("Имя и тип дополнительного поля не могут быть пустыми.");
            if ((field.Type.Equals("select", StringComparison.OrdinalIgnoreCase) || field.Type.Equals("radiobutton", StringComparison.OrdinalIgnoreCase))
                && (field.Variants == null || field.Variants.Count == 0))
            {
                _logger.LogError("Определение поля '{FieldName}' типа '{FieldType}' не содержит вариантов.", field.Name, field.Type);
                throw new ArgumentException($"Требуются варианты для дополнительного поля '{field.Name}' типа '{field.Type}'.");
            }
        }

        var parameters = new CreateCustomFieldsRequestParams { Data = fieldDefinitions };
        var response = await SendApiRequestAsync(typeof(string), "createCustomFields", parameters, cancellationToken).ConfigureAwait(false);
        var result = response as string;
        _logger.LogInformation("Результат создания/обновления доп. полей: {Result}", result);
        return result;
    }
    #endregion
}