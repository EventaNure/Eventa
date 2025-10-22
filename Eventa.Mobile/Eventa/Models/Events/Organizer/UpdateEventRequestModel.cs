using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eventa.Models.Events.Organizer;

public class UpdateEventRequestModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("organizerId")]
    public string OrganizerId { get; set; } = string.Empty;

    [JsonPropertyName("placeId")]
    public int PlaceId { get; set; }

    [JsonPropertyName("tagIds")]
    public List<int> TagIds { get; set; } = [];

    [JsonPropertyName("dateTimes")]
    public List<DateTime> DateTimes { get; set; } = [];

    [JsonIgnore]
    public byte[]? ImageFile { get; set; }

    [JsonIgnore]
    public string? ImageFileName { get; set; }
}