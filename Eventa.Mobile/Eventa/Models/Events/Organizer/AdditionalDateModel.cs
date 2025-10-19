using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;

namespace Eventa.Models.Events.Organizer;

public partial class AdditionalDateModel : ObservableObject
{
    [ObservableProperty]
    private string? _time;

    [ObservableProperty]
    private string _duration = string.Empty;

    [ObservableProperty]
    private DateTime? _date;

    public DateTime ParsedDateTime
    {
        get
        {
            if (Date is null || string.IsNullOrWhiteSpace(Time))
                return DateTime.MinValue;

            var datePart = Date.Value.Date;
            if (DateTime.TryParseExact(Time,
                                       ["H:mm", "HH:mm", "h:mm tt", "hh:mm tt"],
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out var timePart))
            {
                return datePart.Add(timePart.TimeOfDay);
            }

            return DateTime.MinValue;
        }
    }
}