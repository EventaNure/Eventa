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

    [ObservableProperty]
    private ObservableCollection<string> _availablePlaces = [];
    [ObservableProperty]
    private ObservableCollection<SelectableTag> _availableTags = [];
    [ObservableProperty]
    private ObservableCollection<AdditionalDateModel> _additionalDates = [];

    private string _jwtToken = "";
    private string _organizerId = "";
    private List<PlaceResponseModel> _placesData = [];

    public CreateEditDeleteOrganizerEventViewModel()
    {
        _apiService = new ApiService();

        AvailablePlaces = [];
        AvailableTags = [];
        AdditionalDates = [];
    }

    public async Task LoadPlacesAndTagsAsync()
    {
        try
        {
            IsLoading = true;

            // Fetch places
            var (Success, Message, Data) = await _apiService.GetPlacesAsync();
            if (Success && Data != null)
            {
                _placesData = Data;
                AvailablePlaces.Clear();
                foreach (var place in Data)
                {
                    AvailablePlaces.Add($"{place.Name} - {place.Address}");
                }
            }
            else
            {
                ErrorMessage = Message;
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

            var (Success, Message, Data) = await _apiService.GetEventByIdAsync(eventId, _jwtToken);

            if (!Success || Data == null)
            {
                ErrorMessage = Message;
                return;
            }

            var eventData = Data;

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
                SelectedEventDate = firstDateTime.DateTime.Date;
                EventTime = firstDateTime.DateTime.ToString("HH:mm");
                AdditionalDates.Clear();
                for (int i = 1; i < eventData.DateTimes.Count; i++)
                {
                    var additionalDateTime = eventData.DateTimes[i];
                    AdditionalDates.Add(new AdditionalDateModel
                    {
                        Date = additionalDateTime.DateTime.Date,
                        Time = additionalDateTime.DateTime.ToString("HH:mm"),
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
        SelectedTags?.Clear();
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
                Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp"],
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

                if (OperatingSystem.IsBrowser())
                {
                    // Browser: Upload to server and get URL (skip validation)
                    await using var sourceStream = await file.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();

                    var (Success, Message, ImageUrl) = await _apiService.UploadTempImageAsync(imageBytes, file.Name, _jwtToken);

                    if (Success && ImageUrl != null)
                    {
                        EventImage = ImageUrl;
                        ErrorMessage = string.Empty;
                    }
                    else
                    {
                        ErrorMessage = Message;
                    }
                }
                else
                {
                    // Desktop/Android: Validate and save to local cache
                    await using (var stream = await file.OpenReadAsync())
                    {
                        using var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                        if (bitmap.PixelSize.Width != 256 || bitmap.PixelSize.Height != 256)
                        {
                            ErrorMessage = $"Image must be exactly 256x256 pixels. Selected image is {bitmap.PixelSize.Width}x{bitmap.PixelSize.Height}.";
                            return;
                        }
                    }

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

        string normalizedPrice = TicketPrice.Replace(',', '.');

        if (!double.TryParse(normalizedPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price) || price < 0)
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

            // Prepare image file
            byte[]? imageBytes = null;
            string? imageFileName = null;

            if (!string.IsNullOrEmpty(EventImage))
            {
                // Check if EventImage is a local file path (not a URL)
                if (!EventImage.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !EventImage.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(EventImage))
                {
                    // Load local file
                    imageBytes = await File.ReadAllBytesAsync(EventImage);
                    imageFileName = Path.GetFileName(EventImage);
                }
                else if (EventImage.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         EventImage.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // Download image from server
                    try
                    {
                        var imageData = await _apiService.DownloadImageAsync(EventImage);
                        if (imageData != null && imageData.Length > 0)
                        {
                            imageBytes = imageData;
                            // Extract filename from URL or use a default
                            imageFileName = Path.GetFileName(new Uri(EventImage).LocalPath);
                            if (string.IsNullOrEmpty(imageFileName))
                            {
                                imageFileName = $"event_image_{Guid.NewGuid()}.png";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Failed to download image: {ex.Message}";
                        return;
                    }
                }
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

                var (Success, Message) = await _apiService.UpdateEventAsync(EventId.Value, updateModel, _jwtToken);

                if (Success)
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
                    ErrorMessage = Message;
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

                var (Success, Message) = await _apiService.CreateEventAsync(createModel, _jwtToken);

                if (Success)
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
                    ErrorMessage = Message;
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

        // Show confirmation dialog and await response
        bool confirmed = await DialogControl.Instance.Show(
            title: "Delete Event",
            description: "Are you sure you want to delete this event?\n\nYou won't be able to undo this action.",
            noButton: "No, go back",
            okButton: "Delete"
        );

        if (confirmed)
        {
            try
            {
                IsLoading = true;

                var (Success, Message) = await _apiService.DeleteEventAsync(EventId.Value, _jwtToken);

                if (Success)
                {
                    ErrorMessage = string.Empty;
                    ClearForm();
                    BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCreating = false;

                    await MainPageView.Instance.mainPageViewModel.LoadOrganizerEventsAsync(_jwtToken);
                    await BrowseEventsView.Instance.browseEventsViewModel.ApplyFiltersCommand.ExecuteAsync(null);
                }
                else
                {
                    ErrorMessage = Message;
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