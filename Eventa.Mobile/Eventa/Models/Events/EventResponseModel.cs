using System;
using System.Text.Json.Serialization;

namespace Eventa.Models.Events;

public class EventResponseModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("firstDateTime")]
    public DateTime FirstDateTime { get; set; }

    [JsonPropertyName("lastDateTime")]
    public DateTime LastDateTime { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string Image { get; set; } = string.Empty;
}
