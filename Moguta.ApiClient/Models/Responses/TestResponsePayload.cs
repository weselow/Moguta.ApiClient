namespace Moguta.ApiClient.Models.Responses;

/// <summary>
/// Представляет полезную нагрузку (payload), возвращаемую API методом 'test'.
/// Должна зеркально отражать параметры, отправленные в запросе.
/// Используем словарь для гибкости, т.к. фактический тип значений может варьироваться (числа, строки, bool).
/// </summary>
public class TestResponsePayload : Dictionary<string, object> { }