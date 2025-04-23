using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moguta.ApiClient.Abstractions; // Подключаем интерфейс
using System.Net; // Для DecompressionMethods

namespace Moguta.ApiClient.Extensions;

/// <summary>
/// Предоставляет методы расширения для регистрации клиента <see cref="IMogutaApiClient"/> и связанных служб
/// в <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет типизированный HttpClient <see cref="IMogutaApiClient"/> в указанную <see cref="IServiceCollection"/>,
    /// сконфигурированный с помощью делегата действия.
    /// </summary>
    /// <param name="services">Коллекция <see cref="IServiceCollection"/> для добавления служб.</param>
    /// <param name="configureOptions">Делегат действия для конфигурации <see cref="MogutaApiClientOptions"/>.</param>
    /// <returns>Объект <see cref="IHttpClientBuilder"/> для дальнейшей конфигурации HttpClient.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если services или configureOptions равны null.</exception>
    public static IHttpClientBuilder AddMogutaApiClient(
        this IServiceCollection services,
        Action<MogutaApiClientOptions> configureOptions)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        // Конфигурация опций
        services.Configure(configureOptions);
        // Добавляем валидацию опций на основе атрибутов DataAnnotations
        services.AddOptions<MogutaApiClientOptions>()
                .ValidateDataAnnotations()
                .ValidateOnStart(); // Проверять опции при старте приложения

        // Регистрация HttpClient и типизированного клиента
        // Регистрируем реализацию против интерфейса
        return services.AddHttpClient<IMogutaApiClient, MogutaApiClient>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                // BaseAddress и Timeout настраиваются в конструкторе MogutaApiClient из опций
            })
            // Настраиваем обработчик сообщений
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Включаем автоматическую распаковку GZip/Deflate, если сервер их поддерживает
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                // Здесь можно добавить другие настройки обработчика при необходимости
                // (например, прокси, обработка сертификатов)
            });
        // Здесь можно добавить политики отказоустойчивости, например, Polly
        // .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(...))
    }

    /// <summary>
    /// Добавляет типизированный HttpClient <see cref="IMogutaApiClient"/> в указанную <see cref="IServiceCollection"/>,
    /// сконфигурированный с использованием секции конфигурации приложения (например, appsettings.json).
    /// </summary>
    /// <param name="services">Коллекция <see cref="IServiceCollection"/> для добавления служб.</param>
    /// <param name="configurationSectionPath">Путь к секции конфигурации для <see cref="MogutaApiClientOptions"/>. По умолчанию "MogutaApi".</param>
    /// <returns>Объект <see cref="IHttpClientBuilder"/> для дальнейшей конфигурации HttpClient.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если services равен null.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если configurationSectionPath null или пуст.</exception>
    /// <exception cref="OptionsValidationException">Выбрасывается при старте приложения, если конфигурация не проходит валидацию на основе атрибутов в <see cref="MogutaApiClientOptions"/>.</exception>
    public static IHttpClientBuilder AddMogutaApiClient(
        this IServiceCollection services,
        string configurationSectionPath = "MogutaApi")
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (string.IsNullOrWhiteSpace(configurationSectionPath)) throw new ArgumentNullException(nameof(configurationSectionPath));

        // Привязка опций из конфигурации
        services.AddOptions<MogutaApiClientOptions>()
                .BindConfiguration(configurationSectionPath) // Привязать к секции
                .ValidateDataAnnotations() // Включить валидацию по атрибутам
                .ValidateOnStart(); // Проверять при старте

        // Регистрация HttpClient и типизированного клиента
        return services.AddHttpClient<IMogutaApiClient, MogutaApiClient>()
            .ConfigureHttpClient((serviceProvider, client) => { /* Настройка в конструкторе */ })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
    }
}