using System.Text.Json.Serialization;
using Moguta.ApiClient.Infrastructure.Converters;
using Moguta.ApiClient.Models.Common;

public class Property
{
    // Поля из ответа API getProduct
    [JsonPropertyName("id")]
    [JsonConverter(typeof(StringToLongConverter))]
    public long Id { get; set; } // Сделаем non-nullable

    /// <summary>
    /// название характеристики
    /// </summary>
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>
    /// тип хварактеристики
    /// </summary>
    /// <remarks>В справке встретились: string, textarea</remarks>
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty; // size, color, assortmentCheckBox etc.

    /// <summary>
    /// Список значений этой характеристики
    /// </summary>
    [JsonPropertyName("data")]
    [JsonConverter(typeof(NullableListConverter<PropertyData>))] 
    public List<PropertyData>? Data { get; set; } 


    [JsonPropertyName("all_category")] // Непонятно, bool или int
    [JsonConverter(typeof(IntToBoolConverter))] // Предпол. bool
    public bool? AllCategory { get; set; }

    [JsonPropertyName("activity")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool Activity { get; set; }

    [JsonPropertyName("sort")]
    [JsonConverter(typeof(StringToIntConverter))]
    public int Sort { get; set; }

    [JsonPropertyName("filter")]
    [JsonConverter(typeof(IntToBoolConverter))]
    public bool Filter { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type_filter")]
    public string? TypeFilter { get; set; } // checkbox

    [JsonPropertyName("1c_id")]
    public string? ExternalId1C { get; set; }

    [JsonPropertyName("plugin")]
    public string? Plugin { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("group_id")]
    [JsonConverter(typeof(StringToNullableLongConverter))] // Может быть "0" или ID группы
    public long? GroupId { get; set; }

    // Поля из запроса importProduct (оставляем на случай использования DTO для отправки)
    [JsonPropertyName("value")] public string Value { get; set; } = string.Empty;
    // Убираем value, т.к. при получении оно внутри data[].name

    // Убираем поля, которых нет в ответе getProduct (type_view, property_margin, prop_val_id)
    // [JsonPropertyName("type_view")] public string? TypeView { get; set; }
    // [JsonPropertyName("property_margin")] public decimal? PropertyMargin { get; set; }
    // [JsonPropertyName("prop_val_id")] public long? PropertyValueId { get; set; }
    // [JsonPropertyName("property_id")] public long? PropertyId { get; set; } // Дублирует Id
}