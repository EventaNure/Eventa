using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Controls;
using Eventa.Models.Events.Organizer;
using Eventa.Services;
using Eventa.Views.Events;
using Eventa.Views.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Events;

public partial class CreateEditDeleteOrganizerEventViewModel : ObservableObject
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
    private ObservableCollection<SelectableTag>? _selectedTags;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private DateTime? _selectedEventDate;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int? _eventId;

    public ObservableCollection<string> AvailablePlaces { get; }
    public ObservableCollection<SelectableTag> AvailableTags { get; }
    public ObservableCollection<AdditionalDateModel> AdditionalDates { get; }

    private string _jwtToken = "";
    private string _organizerId = "";
    private List<PlaceResponseModel> _placesData = new();

    public CreateEditDeleteOrganizerEventViewModel()
    {
        _apiService = new ApiService();

        AvailablePlaces = new ObservableCollection<string>();
        AvailableTags = new ObservableCollection<SelectableTag>();
        AdditionalDates = new ObservableCollection<AdditionalDateModel>();

        // Load places and tags on initialization
        _ = LoadPlacesAndTagsAsync();
    }

    private async Task LoadPlacesAndTagsAsync()
    {
        try
        {
            IsLoading = true;

            // Fetch places
            var placesResult = await _apiService.GetPlacesAsync();
            if (placesResult.Success && placesResult.Data != null)
            {
                _placesData = placesResult.Data;
                AvailablePlaces.Clear();
                foreach (var place in placesResult.Data)
                {
                    AvailablePlaces.Add($"{place.Name} - {place.Address}");
                }
            }
            else
            {
                ErrorMessage = placesResult.Message;
            }

            // Fetch tags
            var tagsResult = await _apiService.GetAllTagsAsync();
            if (tagsResult.Success && tagsResult.Data != null)
            {
                AvailableTags.Clear();
                foreach (var tag in tagsResult.Data)
                {
                    AvailableTags.Add(new SelectableTag
                    {
                        Id = tag.Id,
                        Name = tag.Name,
                        IsSelected = false
                    });
                }
            }
            else
            {
                ErrorMessage = tagsResult.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadEventAsync(int eventId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _apiService.GetEventByIdAsync(eventId, _jwtToken);

            if (!result.Success || result.Data == null)
            {
                ErrorMessage = result.Message;
                return;
            }

            var eventData = result.Data;

            IsEditMode = true;
            EventId = eventData.Id;
            EventName = eventData.Title;
            Description = eventData.Description;
            TicketPrice = eventData.Price.ToString("F2");

            // Handle duration - it comes as TimeSpan
            EventDuration = ((int)eventData.Duration.TotalMinutes).ToString();
            EventImage = eventData.ImageUrl;

            // Set the selected place by matching the place name
            var matchingPlace = _placesData.FirstOrDefault(p => p.Name == eventData.PlaceName);
            if (matchingPlace != null)
            {
                SelectedPlace = $"{matchingPlace.Name} - {matchingPlace.Address}";
            }

            if (eventData.DateTimes != null && eventData.DateTimes.Count > 0)
            {
                var firstDateTime = eventData.DateTimes[0];
                SelectedEventDate = firstDateTime.Date;
                EventTime = firstDateTime.ToString("HH:mm");

                AdditionalDates.Clear();
                for (int i = 1; i < eventData.DateTimes.Count; i++)
                {
                    var additionalDateTime = eventData.DateTimes[i];
                    AdditionalDates.Add(new AdditionalDateModel
                    {
                        Date = additionalDateTime.Date,
                        Time = additionalDateTime.ToString("HH:mm"),
                        Duration = ((int)eventData.Duration.TotalMinutes).ToString()
                    });
                }
            }

            // Set selected tags
            SelectedTags = [];
            if (eventData.Tags != null)
            {
                await Task.Delay(100);
                foreach (var tag in eventData.Tags)
                {
                    SelectedTags.Add(new SelectableTag() { Id = tag.Id, IsSelected = tag.IsSelected, Name = tag.Name });
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load event: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
        EventImage = null;
        SelectedEventDate = null;
        AdditionalDates.Clear();
        ErrorMessage = string.Empty;

        // Deselect all tags
        foreach (var tag in AvailableTags)
        {
            tag.IsSelected = false;
        }
    }

    [RelayCommand]
    private async Task UploadImage()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(CreateEditDeleteOrganizerEventView.Instance.UploadImageButton);
            if (topLevel == null)
            {
                ErrorMessage = "Unable to access file picker";
                return;
            }

            var storageProvider = topLevel.StorageProvider;

            var fileTypeFilter = new FilePickerFileType("Images")
            {
                Patterns = ["*.jpg", "*.jpeg"],
                MimeTypes = ["image/*"]
            };

            var options = new FilePickerOpenOptions
            {
                Title = "Select an Event Image",
                AllowMultiple = false,
                FileTypeFilter = [fileTypeFilter]
            };

            var result = await storageProvider.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var file = result[0];

                var cacheDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Eventa",
                    "ImageCache"
                );

                Directory.CreateDirectory(cacheDirectory);

                var fileExtension = Path.GetExtension(file.Name) ?? ".jpg";
                var fileName = $"event_image_{Guid.NewGuid()}{fileExtension}";
                var destinationPath = Path.Combine(cacheDirectory, fileName);

                await using (var sourceStream = await file.OpenReadAsync())
                await using (var destinationStream = File.Create(destinationPath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                EventImage = destinationPath;
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to upload image: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddDate()
    {
        var newDate = new AdditionalDateModel
        {
            Time = string.Empty,
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

        // Get the actual place ID from the selected place string
        var selectedPlaceData = _placesData.FirstOrDefault(p =>
            $"{p.Name} - {p.Address}" == SelectedPlace);

        if (selectedPlaceData == null)
        {
            ErrorMessage = "Invalid place selected";
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

        if (SelectedTags == null || SelectedTags.Count == 0)
        {
            ErrorMessage = "At least one tag is required";
            return;
        }

        if (!TimeSpan.TryParse(EventTime, out _))
        {
            ErrorMessage = "Invalid time format. Use HH:MM";
            return;
        }

        if (!int.TryParse(EventDuration, out var duration) || duration <= 0)
        {
            ErrorMessage = "Invalid duration. Enter minutes as a number";
            return;
        }

        // Validate additional dates
        if (AdditionalDates != null && AdditionalDates.Count > 0)
        {
            for (int i = 0; i < AdditionalDates.Count; i++)
            {
                var additionalDate = AdditionalDates[i];

                if (string.IsNullOrWhiteSpace(additionalDate.Time))
                {
                    ErrorMessage = $"Additional date #{i + 1}: Time is required";
                    return;
                }

                if (!TimeSpan.TryParse(additionalDate.Time, out _))
                {
                    ErrorMessage = $"Additional date #{i + 1}: Invalid time format. Use HH:MM";
                    return;
                }

                if (string.IsNullOrWhiteSpace(additionalDate.Duration))
                {
                    ErrorMessage = $"Additional date #{i + 1}: Duration is required";
                    return;
                }

                if (!int.TryParse(additionalDate.Duration, out var addDuration) || addDuration <= 0)
                {
                    ErrorMessage = $"Additional date #{i + 1}: Invalid duration. Enter minutes as a number";
                    return;
                }

                if (!additionalDate.Date.HasValue)
                {
                    ErrorMessage = $"Additional date #{i + 1}: Date is required";
                    return;
                }

                var parsedDateTime = additionalDate.ParsedDateTime;
                if (parsedDateTime == DateTime.MinValue)
                {
                    ErrorMessage = $"Additional date #{i + 1}: Invalid date or time combination";
                    return;
                }
            }
        }

        try
        {
            IsLoading = true;

            // Prepare DateTimes list (main event + additional dates)
            var dateTimes = new List<DateTime>();

            // Parse main event datetime
            var mainDateTime = DateTime.Parse($"{SelectedEventDate.Value:yyyy-MM-dd} {EventTime}");
            dateTimes.Add(mainDateTime);

            // Add additional dates
            if (AdditionalDates != null && AdditionalDates.Count > 0)
            {
                foreach (var additionalDate in AdditionalDates)
                {
                    dateTimes.Add(additionalDate.ParsedDateTime);
                }
            }

            // Prepare image file if exists
            byte[]? imageBytes = null;
            string? imageFileName = null;

            if (!string.IsNullOrEmpty(EventImage) && File.Exists(EventImage))
            {
                imageBytes = await File.ReadAllBytesAsync(EventImage);
                // Use original filename to preserve correct extension for validation
                imageFileName = Path.GetFileName(EventImage);
            }

            if (IsEditMode && EventId.HasValue)
            {
                // Update event
                var updateModel = new UpdateEventRequestModel
                {
                    Title = EventName,
                    Description = Description,
                    Price = price,
                    Duration = TimeSpan.FromMinutes(duration),
                    OrganizerId = _organizerId,
                    PlaceId = selectedPlaceData.Id,
                    TagIds = [.. SelectedTags.Select(t => t.Id)],
                    DateTimes = dateTimes,
                    ImageFile = imageBytes,
                    ImageFileName = imageFileName
                };

                var result = await _apiService.UpdateEventAsync(EventId.Value, updateModel, _jwtToken);

                if (result.Success)
                {
                    ErrorMessage = string.Empty;
                    ClearForm();
                    BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCreating = false;

                    // Refresh the events list
                    await MainPageView.Instance.mainPageViewModel.LoadOrganizerEventsAsync(_jwtToken);
                    await BrowseEventsView.Instance.browseEventsViewModel.ApplyFiltersCommand.ExecuteAsync(null);
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            else
            {
                // Create event
                var createModel = new CreateEventRequestModel
                {
                    Title = EventName,
                    Description = Description,
                    Price = price,
                    Duration = TimeSpan.FromMinutes(duration),
                    OrganizerId = _organizerId,
                    PlaceId = selectedPlaceData.Id,
                    TagIds = [.. SelectedTags.Select(t => t.Id)],
                    DateTimes = dateTimes,
                    ImageFile = imageBytes,
                    ImageFileName = imageFileName
                };

                var result = await _apiService.CreateEventAsync(createModel, _jwtToken);

                if (result.Success)
                {
                    ErrorMessage = string.Empty;
                    ClearForm();
                    BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCreating = false;

                    // Refresh the events list
                    await MainPageView.Instance.mainPageViewModel.LoadOrganizerEventsAsync(_jwtToken);
                    await BrowseEventsView.Instance.browseEventsViewModel.ApplyFiltersCommand.ExecuteAsync(null);
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
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
        if (!IsEditMode || !EventId.HasValue)
            return;

        // TODO: Show confirmation dialog
        // For now, proceeding directly

        try
        {
            IsLoading = true;

            var result = await _apiService.DeleteEventAsync(EventId.Value, _jwtToken);

            if (result.Success)
            {
                ErrorMessage = string.Empty;
                ClearForm();
                BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCreating = false;

                // Refresh the events list
                await MainPageView.Instance.mainPageViewModel.LoadOrganizerEventsAsync(_jwtToken);
            }
            else
            {
                ErrorMessage = result.Message;
            }
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
    }

    public void InsertFormData(string jwtToken, string organizerId, int? eventId = null)
    {
        _jwtToken = jwtToken;
        _organizerId = organizerId;

        if (eventId.HasValue && eventId.Value > 0)
        {
            _ = LoadEventAsync(eventId.Value);
        }
    }
}