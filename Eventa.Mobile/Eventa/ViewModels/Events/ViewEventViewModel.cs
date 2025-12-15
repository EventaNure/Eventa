using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Comments;
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
    private string? _prices;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _nothingFound;

    [ObservableProperty]
    private string? _organizerName;

    [ObservableProperty]
    private double _averageRating;

    [ObservableProperty]
    private ObservableCollection<EventDateTimes> _dateTimes = [];

    [ObservableProperty]
    private ObservableCollection<CommentDataModel> _comments = [];

    // Rounded rating for star display
    public int RoundedAverageRating => (int)Math.Round(AverageRating);

    public void InsertFormData(EventDetailsResponseModel eventDetails)
    {
        Title = eventDetails.Title;
        Description = eventDetails.Description;
        Date = eventDetails.DateTimes.Count > 0 ? eventDetails.DateTimes[0].DateTime : null;
        Image = eventDetails.ImageUrl;
        Address = eventDetails.PlaceAddress;

        if (eventDetails.MinPrice == eventDetails.MaxPrice)
        {
            Prices = $"₴{eventDetails.MinPrice}";
        }
        else
        {
            Prices = $"₴{eventDetails.MinPrice} - ₴{eventDetails.MaxPrice}";
        }

        OrganizerName = eventDetails.OrganizerName;
        AverageRating = eventDetails.AverageRating;

        DateTimes.Clear();
        foreach (var dateTime in eventDetails.DateTimes)
        {
            DateTimes.Add(dateTime);
        }

        Comments.Clear();
        foreach (var comment in eventDetails.Comments)
        {
            Comments.Add(comment);
        }

        // Notify that RoundedAverageRating may have changed
        OnPropertyChanged(nameof(RoundedAverageRating));
    }

    public void ClearFormData()
    {
        Title = null;
        Description = null;
        Date = null;
        Image = null;
        Address = null;
        Prices = null;
        OrganizerName = null;
        AverageRating = 0.0;
        DateTimes.Clear();
        Comments.Clear();
        ErrorMessage = null;
        NothingFound = false;
        OnPropertyChanged(nameof(RoundedAverageRating));
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