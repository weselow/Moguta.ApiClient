using Moguta.ApiClient.Models.Common;
using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Responses;

public class GetProductResponsePayload
{
    [JsonPropertyName("countProduct")] // Имя верное
    [JsonConverter(typeof(StringToLongConverter))] // Конвертер верный
    public long TotalCount { get; set; }

    [JsonPropertyName("products")] // Имя верное
    public List<MogutaProduct> Products { get; set; } = [];

    [JsonPropertyName("page")]
    public int? Page { get; set; } // Стандартный парсер справится

    [JsonPropertyName("count")]
    public int? Count { get; set; } // Стандартный парсер справится

    // Добавляем Variants и Property флаги, которые возвращает API
    [JsonPropertyName("variants")]
    [JsonConverter(typeof(IntToBoolConverter))] // Приходит как 1
    public bool IncludesVariants { get; set; }

    [JsonPropertyName("property")]
    [JsonConverter(typeof(IntToBoolConverter))] // Приходит как 1
    public bool IncludesProperties { get; set; }
}