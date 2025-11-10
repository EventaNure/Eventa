using System;
using System.Text.Json.Serialization;

namespace Eventa.Models.Events.Organizer;

public class EventDateTimes
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("dateTime")]
    public DateTime DateTime { get; set; }
}