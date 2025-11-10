using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events.Organizer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Eventa.ViewModels.Events;

public partial class ViewEventViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;
    [ObservableProperty]
    private string? _description;
    [ObservableProperty]
    private DateTime? _date;
    [ObservableProperty]
    private string? _image;
    [ObservableProperty]
    private string? _address;
    [ObservableProperty]
    private string? prices;
    [ObservableProperty]
    private ObservableCollection<EventDateTimes> _dateTimes = new();

    public void InsertFormData(string title, string description, DateTime date, string? image, string address, string prices, List<EventDateTimes> dateTimes)
    {
        Title = title;
        Description = description;
        Date = date;
        Image = image;
        Address = address;
        Prices = prices;
        DateTimes.Clear();
        foreach (var dateTime in dateTimes)
        {
            DateTimes.Add(dateTime);
        }
    }

    [RelayCommand]
    public void SelectDateTime(EventDateTimes? selectedDateTime)
    {
        if (selectedDateTime == null)
            return;


    }
}