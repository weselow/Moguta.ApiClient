using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;

namespace Moguta.ApiClient.Models.Common;

public class MogutaPropertyData
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringToLongConverter))]
    public long Id { get; set; }

    [JsonPropertyName("prop_id")]
    [JsonConverter(typeof(StringToLongConverter))]
    public long PropertyId { get; set; }

    [JsonPropertyName("prop_data_id")]
    [JsonConverter(typeof(StringToLongConverter))]
    public long ValueId { get; set; } // ID значения характеристики

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty; // Значение характеристики

    [JsonPropertyName("margin")]
    [JsonConverter(typeof(StringToNullableDecimalConverter))] // Пустая строка
    public decimal? Margin { get; set; }
}