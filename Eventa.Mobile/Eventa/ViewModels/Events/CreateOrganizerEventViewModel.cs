using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events.Organizer;
using Eventa.Services;
using Eventa.Views.Events;

namespace Eventa.ViewModels.Events;

public partial class CreateOrganizerEventViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string? _eventImage;

    [ObservableProperty]
    private string _eventName = string.Empty;

    [ObservableProperty]
    private string? _selectedPlace;

    [ObservableProperty]
    private string _eventTime = string.Empty;

    [ObservableProperty]
    private string _eventDuration = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _ticketPrice = string.Empty;

    [ObservableProperty]
    private string? _selectedTag;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private DateTime? _selectedEventDate;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string? _eventId;

    public ObservableCollection<string> AvailablePlaces { get; }
    public ObservableCollection<string> AvailableTags { get; }
    public ObservableCollection<AdditionalDateModel> AdditionalDates { get; }

    public CreateOrganizerEventViewModel()
    {
        _apiService = new ApiService();

        // Initialize collections with sample data
        AvailablePlaces = new ObservableCollection<string>
        {
            "Convention Center",
            "Stadium",
            "Theater",
            "Arena",
            "Conference Hall",
            "Outdoor Park",
            "Concert Hall",
            "Exhibition Center"
        };

        AvailableTags = new ObservableCollection<string>
        {
            "Music",
            "Sports",
            "Technology",
            "Art",
            "Food & Drink",
            "Business",
            "Education",
            "Entertainment",
            "Health & Wellness",
            "Community"
        };

        AdditionalDates = new ObservableCollection<AdditionalDateModel>();
    }

    public void LoadEvent(OrganizerEventResponseModel eventData)
    {
        IsEditMode = true;
        EventId = eventData.Id.ToString();
        EventName = eventData.Title ?? string.Empty;
        // Map other properties from eventData to view model properties
        // SelectedPlace = eventData.Place;
        // EventTime = eventData.Time;
        // EventDuration = eventData.Duration.ToString();
        // Description = eventData.Description;
        // TicketPrice = eventData.Price;
        // SelectedTag = eventData.Tag;
        // EventImage = eventData.ImageUrl;
    }

    public void ClearForm()
    {
        IsEditMode = false;
        EventId = null;
        EventName = string.Empty;
        SelectedPlace = null;
        EventTime = string.Empty;
        EventDuration = string.Empty;
        Description = string.Empty;
        TicketPrice = string.Empty;
        SelectedTag = null;
        EventImage = null;
        SelectedEventDate = null;
        AdditionalDates.Clear();
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task UploadImage()
    {
        try
        {
            // TODO: Implement file picker for image selection
            // For now, this is a placeholder
            // You'll need to use Avalonia's file picker or a platform-specific service

            // Example implementation:
            // var file = await _filePickerService.PickImageAsync();
            // if (file != null)
            // {
            //     EventImage = file.Path;
            //     // Or upload to server and get URL
            //     // EventImage = await _apiService.UploadImageAsync(file);
            // }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to upload image: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SelectEventDate()
    {
        // TODO: Implement date picker dialog
        // This should open a calendar dialog and set SelectedEventDate
        // For now, this is a placeholder

        // Example:
        // var date = await _dialogService.ShowDatePickerAsync();
        // if (date.HasValue)
        // {
        //     SelectedEventDate = date.Value;
        // }
    }

    [RelayCommand]
    private void AddDate()
    {
        var newDate = new AdditionalDateModel
        {
            Place = AvailablePlaces.FirstOrDefault(),
            Duration = string.Empty,
            Date = null
        };

        AdditionalDates.Add(newDate);
    }

    [RelayCommand]
    private void RemoveDate(AdditionalDateModel date)
    {
        if (date != null)
        {
            AdditionalDates.Remove(date);
        }
    }

    [RelayCommand]
    private async Task SaveEvent()
    {
        ErrorMessage = string.Empty;

        // Validate required fields
        if (string.IsNullOrWhiteSpace(EventName))
        {
            ErrorMessage = "Event name is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedPlace))
        {
            ErrorMessage = "Event place is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(EventTime))
        {
            ErrorMessage = "Event time is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(EventDuration))
        {
            ErrorMessage = "Event duration is required";
            return;
        }

        if (!SelectedEventDate.HasValue)
        {
            ErrorMessage = "Event date is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "Description is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(TicketPrice))
        {
            ErrorMessage = "Ticket price is required";
            return;
        }

        if (!decimal.TryParse(TicketPrice, out var price) || price < 0)
        {
            ErrorMessage = "Invalid ticket price";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedTag))
        {
            ErrorMessage = "Tag is required";
            return;
        }

        // Validate time format (HH:MM)
        if (!TimeSpan.TryParse(EventTime, out _))
        {
            ErrorMessage = "Invalid time format. Use HH:MM";
            return;
        }

        // Validate duration is numeric
        if (!int.TryParse(EventDuration, out var duration) || duration <= 0)
        {
            ErrorMessage = "Invalid duration. Enter minutes as a number";
            return;
        }

        try
        {
            IsLoading = true;

            // Create event object
            var eventData = new
            {
                Id = EventId,
                Name = EventName,
                Place = SelectedPlace,
                Time = EventTime,
                Duration = duration,
                Date = SelectedEventDate.Value,
                Description = Description,
                TicketPrice = price,
                Tag = SelectedTag,
                ImageUrl = EventImage,
                AdditionalDates = AdditionalDates.Select(d => new
                {
                    d.Place,
                    Duration = int.TryParse(d.Duration, out var dur) ? dur : 0,
                    d.Date
                }).ToList()
            };

            if (IsEditMode)
            {
                // TODO: Call API service to update event
                // await _apiService.UpdateEventAsync(EventId, eventData);
            }
            else
            {
                // TODO: Call API service to create event
                // await _apiService.CreateEventAsync(eventData);
            }

            // Simulate API call
            await Task.Delay(1000);

            // Notify success and clear form
            ClearForm();

            // TODO: Raise event or callback to notify parent view
            // OnEventSaved?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save event: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteEvent()
    {
        if (!IsEditMode || string.IsNullOrEmpty(EventId))
            return;

        // TODO: Show confirmation dialog
        // var confirmed = await _dialogService.ShowConfirmationAsync("Delete Event", "Are you sure you want to delete this event?");
        // if (!confirmed)
        //     return;

        try
        {
            IsLoading = true;

            // TODO: Call API service to delete event
            // await _apiService.DeleteEventAsync(EventId);

            // Simulate API call
            await Task.Delay(1000);

            ClearForm();

            // TODO: Raise event or callback to notify parent view
            // OnEventDeleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete event: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        ClearForm();
        BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCreating = false;
        // TODO: Raise event or callback to notify parent view
        // OnCancelled?.Invoke();
    }
}