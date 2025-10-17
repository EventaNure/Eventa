using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Eventa.Models.Events.Organizer;

public partial class AdditionalDateModel : ObservableObject
{
    [ObservableProperty]
    private string? _place;

    [ObservableProperty]
    private string _duration = string.Empty;

    [ObservableProperty]
    private DateTime? _date;
}
