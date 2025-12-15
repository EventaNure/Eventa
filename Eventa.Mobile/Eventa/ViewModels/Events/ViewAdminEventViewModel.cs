using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Controls;
using Eventa.Models.Events.Organizer;
using Eventa.Services;
using Eventa.Views.Events;
using Eventa.Views.Main;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Events;

public partial class ViewAdminEventViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();

    [ObservableProperty]
    private int _eventId;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _image;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private string? _prices;

    [ObservableProperty]
    private string? _dateRange;

    [ObservableProperty]
    private string? _organizerName;

    [ObservableProperty]
    private string? _organizationName;

    [ObservableProperty]
    private double _averageRating;

    [ObservableProperty]
    private AsyncRelayCommand _approveEventCommand;

    [ObservableProperty]
    private AsyncRelayCommand _denyEventCommand;

    [ObservableProperty]
    private RelayCommand _goBackCommand;

    // Rounded rating for star display
    public int RoundedAverageRating => (int)Math.Round(AverageRating);

    public ViewAdminEventViewModel()
    {
        _approveEventCommand = new AsyncRelayCommand(ApproveEventAsync);
        _denyEventCommand = new AsyncRelayCommand(DenyEventAsync);
        _goBackCommand = new RelayCommand(GoBack);
    }

    public void InsertFormData(EventDetailsResponseModel eventDetails)
    {
        EventId = eventDetails.Id;
        Title = eventDetails.Title;
        Description = eventDetails.Description;
        Image = eventDetails.ImageUrl;
        Address = eventDetails.PlaceAddress;
        OrganizerName = eventDetails.OrganizerName;
        AverageRating = eventDetails.AverageRating;

        if (eventDetails.MinPrice == eventDetails.MaxPrice)
        {
            Prices = $"₴{eventDetails.MinPrice}";
        }
        else
        {
            Prices = $"₴{eventDetails.MinPrice} - ₴{eventDetails.MaxPrice}";
        }

        if (eventDetails.DateTimes.Count > 0)
        {
            var firstDate = eventDetails.DateTimes.First().DateTime;
            var lastDate = eventDetails.DateTimes.Last().DateTime;

            if (firstDate.Date == lastDate.Date)
            {
                DateRange = firstDate.ToString("dd MMM yyyy, HH:mm");
            }
            else
            {
                DateRange = $"{firstDate:dd MMM yyyy} - {lastDate:dd MMM yyyy}";
            }
        }

        // Notify that RoundedAverageRating may have changed
        OnPropertyChanged(nameof(RoundedAverageRating));
    }

    public void ClearFormData()
    {
        EventId = 0;
        Title = null;
        Description = null;
        Image = null;
        Address = null;
        Prices = null;
        DateRange = null;
        OrganizerName = null;
        OrganizationName = null;
        AverageRating = 0.0;
        OnPropertyChanged(nameof(RoundedAverageRating));
    }

    private void GoBack()
    {
        MainPageView.Instance.mainPageViewModel.IsCarouselVisible = false;
        MainPageView.Instance.mainPageViewModel.IsBrowsingEventsAsOrganizer = true;
        MainPageView.Instance.mainPageViewModel.CurrentPage = BrowseAdminEventsView.Instance;
    }

    private async Task ApproveEventAsync()
    {
        var result = await DialogControl.Instance.Show(
            "Approve Event",
            "Are you sure you want to approve this event?",
            "No, go back",
            "Approve");

        if (!result)
            return;

        string jwtToken = MainPageView.Instance.mainPageViewModel.JwtToken;

        var (success, message) = await _apiService.ApproveEventAsync(EventId, jwtToken);

        if (success)
        {
            await DialogControl.Instance.Show("Success", "Event approved successfully!", "OK");

            // Navigate back to pending events list
            MainPageView.Instance.mainPageViewModel.HeaderTitleClickedCommand.Execute(null);
            await BrowseAdminEventsView.Instance.browseAdminEventsViewModel.LoadPendingEventsAsync();

            GoBack();
        }
        else
        {
            await DialogControl.Instance.Show("Error", message, "OK");
        }
    }

    private async Task DenyEventAsync()
    {
        var result = await DialogControl.Instance.Show(
            "Deny Event",
            "Are you sure you want to deny this event?",
            "No, go back",
            "Deny");

        if (!result)
            return;

        string jwtToken = MainPageView.Instance.mainPageViewModel.JwtToken;

        var (success, message) = await _apiService.DenyEventAsync(EventId, jwtToken);

        if (success)
        {
            await DialogControl.Instance.Show("Success", "Event denied successfully!", "OK");

            // Navigate back to pending events list
            MainPageView.Instance.mainPageViewModel.HeaderTitleClickedCommand.Execute(null);
            await BrowseAdminEventsView.Instance.browseAdminEventsViewModel.LoadPendingEventsAsync();

            GoBack();
        }
        else
        {
            await DialogControl.Instance.Show("Error", message, "OK");
        }
    }
}