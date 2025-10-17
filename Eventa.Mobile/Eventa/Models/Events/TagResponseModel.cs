using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Eventa.Models.Events;

public partial class TagResponseModel : ObservableObject 
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}