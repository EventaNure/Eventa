using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eventa.Models.Events.Organizer;

public class EventDetailsResponseModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("organizerName")]
    public string OrganizerName { get; set; } = string.Empty;

    [JsonPropertyName("placeName")]
    public string PlaceName { get; set; } = string.Empty;

    [JsonPropertyName("placeAddress")]
    public string PlaceAddress { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<TagResponseModel> Tags { get; set; } = [];

    [JsonPropertyName("dateTimes")]
    public List<EventDateTimes> DateTimes { get; set; } = [];

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}