using System;
using System.Text.Json.Serialization;

namespace Eventa.Models.Comments;

public class CommentDataModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("creationDateTime")]
    public DateTime CreationDateTime { get; set; }

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    // Helper property for UI
    public string UserInitial => string.IsNullOrEmpty(UserName) ? "?" : UserName[0].ToString().ToUpper();
}