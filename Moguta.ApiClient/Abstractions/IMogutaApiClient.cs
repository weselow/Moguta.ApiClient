using Moguta.ApiClient.Exceptions;
using Moguta.ApiClient.Models.Common;
using Moguta.ApiClient.Models.Requests;
using Moguta.ApiClient.Models.Responses;

namespace Moguta.ApiClient.Abstractions;

/// <summary>
/// Определяет контракт для взаимодействия с MogutaCMS API.
/// Предоставляет асинхронные методы для управления товарами, категориями, заказами и пользователями.
/// </summary>
public interface IMogutaApiClient
{
    #region Методы Товаров (Product)

    /// <summary>
    /// Импортирует (создает или обновляет) товары в MogutaCMS.
    /// </summary>
    /// <param name="products">Список объектов <see cref="Product"/> для импорта. Рекомендуется не более 100 за раз.</param>
    /// <param name="batchSize">Количество товаров в одном запросе (max 100).</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом (например, "Импортировано: 1 Обновлено: 0 Ошибок: 0").</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список товаров null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> ImportProductAsync(List<Product> products, int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет товары из MogutaCMS по их уникальным идентификаторам.
    /// </summary>
    /// <param name="productIds">Список ID товаров для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом (например, "Удалено: 2").</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список ID товаров null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> DeleteProductAsync(List<long> productIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает товары из MogutaCMS на основе указанных критериев.
    /// </summary>
    /// <param name="requestParams">Параметры для фильтрации (по ID, артикулу, названию) или пагинации. Включает флаги для получения вариантов и характеристик.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит список объектов <see cref="Product"/>, соответствующих критериям, или <c>null</c>, если ответ API пуст.</returns>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<List<Product>?> GetProductAsync(GetProductRequestParams requestParams, CancellationToken cancellationToken = default);
    #endregion

    #region Методы Категорий (Category)
    /// <summary>
    /// Получает категории из MogutaCMS на основе указанных критериев.
    /// </summary>
    /// <param name="requestParams">Параметры для фильтрации (по ID, URL) или пагинации.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит список объектов <see cref="Category"/>, соответствующих критериям, или <c>null</c>, если ответ API пуст.</returns>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<List<Category>?> GetCategoryAsync(GetCategoryRequestParams requestParams, CancellationToken cancellationToken = default);

    /// <summary>
    /// Импортирует (создает или обновляет) категории в MogutaCMS.
    /// </summary>
    /// <param name="categories">Список объектов <see cref="Category"/> для импорта. Рекомендуется не более 100 за раз.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список категорий null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> ImportCategoryAsync(List<Category> categories, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет категории из MogutaCMS по их уникальным идентификаторам.
    /// </summary>
    /// <param name="categoryIds">Список ID категорий для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список ID категорий null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> DeleteCategoryAsync(List<long> categoryIds, CancellationToken cancellationToken = default);
    #endregion

    #region Методы Заказов (Order)
    /// <summary>
    /// Получает заказы из MogutaCMS на основе указанных критериев.
    /// </summary>
    /// <remarks>
    /// Поле 'OrderContent' в возвращаемых объектах <see cref="Order"/> будет содержать необработанную PHP сериализованную строку из API.
    /// Автоматическая десериализация этого поля не поддерживается (кроме случаев, когда API возвращает JSON).
    /// Используйте свойство <see cref="Order.OrderItems"/>, если контент был успешно десериализован как JSON.
    /// </remarks>
    /// <param name="requestParams">Параметры для фильтрации (по ID, номеру, email) или пагинации.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит список объектов <see cref="Order"/>, соответствующих критериям, или <c>null</c>, если ответ API пуст.</returns>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<List<Order>?> GetOrderAsync(GetOrderRequestParams requestParams, CancellationToken cancellationToken = default);

    /// <summary>
    /// Импортирует (создает или обновляет) заказы в MogutaCMS.
    /// </summary>
    /// <remarks>
    /// **Важно:** Передавайте позиции заказа через свойство <c>OrderItems</c> объекта <see cref="Order"/>.
    /// Этот список будет автоматически сериализован в JSON и отправлен в поле 'order_content'.
    /// Этот подход требует, чтобы API MogutaCMS корректно обрабатывал JSON строку в параметре 'order_content' вместо строки PHP serialize.
    /// Проверьте совместимость API перед использованием для создания/обновления позиций заказа.
    /// </remarks>
    /// <param name="orders">Список объектов <see cref="Order"/> для импорта. Рекомендуется не более 100 за раз.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список заказов null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> ImportOrderAsync(List<Order> orders, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет заказы из MogutaCMS по их уникальным идентификаторам.
    /// </summary>
    /// <param name="orderIds">Список ID заказов для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список ID заказов null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> DeleteOrderAsync(List<long> orderIds, CancellationToken cancellationToken = default);
    #endregion

    #region Методы Пользователей (User)
    /// <summary>
    /// Получает пользователей из MogutaCMS на основе указанных критериев (пагинация или список email).
    /// </summary>
    /// <param name="requestParams">Параметры для фильтрации или пагинации.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит список объектов <see cref="User"/>, соответствующих критериям, или <c>null</c>, если ответ API пуст.</returns>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<List<User>?> GetUserAsync(GetUserRequestParams requestParams, CancellationToken cancellationToken = default);

    /// <summary>
    /// Импортирует (создает или обновляет) пользователей в MogutaCMS. Обновление обычно происходит по совпадению email.
    /// </summary>
    /// <param name="users">Список объектов <see cref="User"/> для импорта. Рекомендуется не более 100 за раз.</param>
    /// <param name="enableUpdate">Установите <c>true</c> для разрешения обновления существующих пользователей (по email). Установите <c>false</c> для создания только новых. Установите <c>null</c> для использования поведения API по умолчанию (вероятно, обновление включено).</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список пользователей null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> ImportUserAsync(List<User> users, bool? enableUpdate = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет пользователей из MogutaCMS по их email адресам.
    /// </summary>
    /// <param name="emails">Список email адресов пользователей для удаления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список email null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> DeleteUserAsync(List<string> emails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Находит одного пользователя в MogutaCMS по его email адресу.
    /// </summary>
    /// <param name="email">Email адрес пользователя для поиска.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит найденный объект <see cref="User"/> или <c>null</c>, если пользователь с таким email не существует.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если email null или пуст.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API (кроме ошибок 'не найдено') или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<User?> FindUserAsync(string email, CancellationToken cancellationToken = default);
    #endregion

    #region Служебные Методы
    /// <summary>
    /// Тестирует соединение и аутентификацию с MogutaCMS API.
    /// Отправляет переданный объект параметров (сериализованный в JSON) и ожидает получить ту же структуру в ответе.
    /// Полезно для проверки учетных данных и базовой доступности API.
    /// </summary>
    /// <param name="parameters">Объект с произвольными параметрами для отправки (например, анонимный объект или Dictionary).</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит <see cref="TestResponsePayload"/> (Dictionary&lt;string, object&gt;), зеркально отражающий отправленные параметры, или <c>null</c>, если ответ API пуст.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если объект параметров null.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<TestResponsePayload?> TestConnectionAsync(object parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создает или обновляет определения дополнительных полей для заказов в MogutaCMS.
    /// Обновление полей, вероятно, происходит по совпадению имени поля.
    /// </summary>
    /// <param name="fieldDefinitions">Список объектов <see cref="CustomFieldDefinition"/>, описывающих поля для создания/обновления.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию. Содержит строку ответа API с результатом.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если список определений полей null, пуст или содержит невалидные определения.</exception>
    /// <exception cref="MogutaApiException">Выбрасывается при ошибках на уровне API или сетевых проблемах.</exception>
    /// <exception cref="MogutaApiSignatureException">Выбрасывается при неверной подписи ответа (если проверка включена).</exception>
    Task<string?> CreateOrUpdateOrderCustomFieldsAsync(List<CustomFieldDefinition> fieldDefinitions, CancellationToken cancellationToken = default);
    #endregion
}