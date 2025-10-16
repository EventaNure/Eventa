using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

namespace Eventa.Models.Events.Organizer;

public partial class OrganizerEventResponseModel : ObservableObject
{
    [ObservableProperty]
    [property: JsonPropertyName("id")]
    private int _id;

    [ObservableProperty]
    [property: JsonPropertyName("title")]
    private string _title = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("price")]
    private decimal _price;

    [ObservableProperty]
    [property: JsonPropertyName("firstDateTime")]
    private DateTime _firstDateTime;

    [ObservableProperty]
    [property: JsonPropertyName("lastDateTime")]
    private DateTime _lastDateTime;

    [ObservableProperty]
    [property: JsonPropertyName("address")]
    private string _address = string.Empty;

    [ObservableProperty]
    [property: JsonPropertyName("imageUrl")]
    private string? _imageUrl;

    [ObservableProperty]
    [property: JsonPropertyName("ticketsSold")]
    private int _ticketsSold;
}