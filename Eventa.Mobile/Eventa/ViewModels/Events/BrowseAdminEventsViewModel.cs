using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events.Admin;
using Eventa.Services;
using Eventa.Views.Events;
using Eventa.Views.Main;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Events;

public partial class BrowseAdminEventsViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();

    [ObservableProperty]
    private ObservableCollection<PendingEventListItem> _pendingEvents = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private AsyncRelayCommand<PendingEventListItem> _viewEventDetailsCommand;

    public BrowseAdminEventsViewModel()
    {
        _viewEventDetailsCommand = new AsyncRelayCommand<PendingEventListItem>(ViewEventDetailsAsync);
    }

    public async Task LoadPendingEventsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        PendingEvents.Clear();

        try
        {
            string jwtToken = MainPageView.Instance.mainPageViewModel.JwtToken;
            var (success, message, data) = await _apiService.GetPendingEventsAsync(jwtToken);

            if (success && data != null)
            {
                foreach (var evt in data)
                {
                    PendingEvents.Add(evt);
                }

                if (data.Count == 0)
                {
                    ErrorMessage = "No pending events to review.";
                }
                else
                {
                    // Fetch ratings for all events
                    await FetchRatingsForAllEventsAsync(jwtToken);
                }
            }
            else
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading pending events: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task FetchRatingsForAllEventsAsync(string jwtToken)
    {
        foreach (var evt in PendingEvents)
        {
            await FetchEventRatingAsync(evt, jwtToken);
        }
    }

    private async Task FetchEventRatingAsync(PendingEventListItem evt, string jwtToken)
    {
        try
        {
            var (success, message, data) = await _apiService.GetEventByIdAsync(evt.Id, jwtToken);

            if (success && data != null)
            {
                evt.AverageRating = data.AverageRating;
            }
        }
        catch { }
    }

    private async Task ViewEventDetailsAsync(PendingEventListItem? pendingEvent)
    {
        if (pendingEvent == null)
            return;

        string jwtToken = MainPageView.Instance.mainPageViewModel.JwtToken;
        var (success, message, data) = await _apiService.GetEventByIdAsync(pendingEvent.Id, jwtToken);

        if (!success || data == null)
        {
            ErrorMessage = message;
            return;
        }

        ViewAdminEventView.Instance.viewAdminEventViewModel.InsertFormData(data);
        MainPageView.Instance.mainPageViewModel.IsCarouselVisible = false;
        MainPageView.Instance.mainPageViewModel.IsBrowsingEventsAsOrganizer = true;
        MainPageView.Instance.mainPageViewModel.CurrentPage = ViewAdminEventView.Instance;
    }
}