using System.Text.Json.Serialization;

namespace Moguta.ApiClient.Models.Requests;

// Этот класс строго не обязателен, т.к. метод test принимает любой объект,
// но для ясности можно использовать Dictionary или определить DTO если структура известна.

/// <summary>
/// Представляет произвольные параметры для API метода 'test'.
/// Используем словарь для гибкости.
/// </summary>
public class TestRequestParams : Dictionary<string, object> { }