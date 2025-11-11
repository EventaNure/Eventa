using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events.Organizer;
using Eventa.Services;
using Eventa.Views.Main;
using Eventa.Views.Ordering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Events;

public partial class ViewEventViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();

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
    private string? _errorMessage;
    [ObservableProperty]
    private bool _nothingFound;
    [ObservableProperty]
    private ObservableCollection<EventDateTimes> _dateTimes = [];

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

    public void ClearFormData()
    {
        Title = null;
        Description = null;
        Date = null;
        Image = null;
        Address = null;
        Prices = null;
        DateTimes.Clear();
        ErrorMessage = null;
        NothingFound = false;
    }

    [RelayCommand]
    public async Task SelectDateTime(EventDateTimes? selectedDateTime)
    {
        if (selectedDateTime == null)
            return;

        string jwtToken = MainPageView.Instance.mainPageViewModel.JwtToken;

        NothingFound = false;

        var (Success, Message, Data) = await _apiService.GetFreeSeatsWithHallPlanAsync(selectedDateTime.Id, jwtToken);
        if (!Success || Data == null)
        {
            ErrorMessage = Message;
            NothingFound = true;
            return;
        }

        await SeatOrderView.Instance.seatOrderViewModel.InsertFormData(Data, Title!, selectedDateTime, jwtToken);

        MainPageView.Instance.mainPageViewModel.IsCarouselVisible = false;
        MainPageView.Instance.mainPageViewModel.IsBrowsingEventsAsOrganizer = true;
        MainPageView.Instance.mainPageViewModel.CurrentPage = SeatOrderView.Instance;
    }
}