using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Responses;

/// <summary>
/// Обобщенная обертка для всех ответов от Moguta API.
/// </summary>
/// <typeparam name="T">Тип фактических данных в поле 'response'.</typeparam>
internal class MogutaApiResponse<T> // Делаем internal, т.к. используется только внутри клиента
{
    /// <summary>
    /// Указывает статус запроса. Должно быть "OK" для успешного выполнения.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Фактические данные ответа для конкретного API метода.
    /// Тип зависит от вызванного метода. Может быть null, если статус не "OK".
    /// </summary>
    [JsonPropertyName("response")]
    public T? Response { get; set; }

    /// <summary>
    /// Код ошибки, если статус не "OK".
    /// 1 - Неверный токен, 2 - Ошибка вызова метода, 3 - API не настроено.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; } // Тип может быть string или int, обрабатываем как string

    /// <summary>
    /// Подпись ответа, сгенерированная сервером. Используется для валидации.
    /// </summary>
    [JsonPropertyName("sign")]
    public string? Sign { get; set; }

    /// <summary>
    /// Время обработки запроса на сервере (информационное поле).
    /// </summary>
    [JsonPropertyName("workTime")]
    public string? WorkTime { get; set; }

    /// <summary>
    /// Необязательное поле для сообщений об ошибках, когда статус не OK,
    /// но сообщение передается в поле 'message' или 'response' (если response - строка).
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}