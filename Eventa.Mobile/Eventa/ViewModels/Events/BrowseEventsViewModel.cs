using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events;
using Eventa.Services;
using Eventa.Views.Events;
using Eventa.Views.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Events;

public partial class BrowseEventsViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    public ObservableCollection<TagResponseModel> Tags { get; set; }
    public ObservableCollection<EventResponseModel> Events { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _nothingFound;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private AsyncRelayCommand _loadTagsCommand;
    [ObservableProperty]
    private AsyncRelayCommand _applyFiltersCommand;

    public BrowseEventsViewModel()
    {
        _apiService = new ApiService();
        Tags = [];
        Events = [];

        _loadTagsCommand = new AsyncRelayCommand(LoadTagsAsync);
        _applyFiltersCommand = new AsyncRelayCommand(ApplyFiltersAsync);

        Tags.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (TagResponseModel tag in e.NewItems)
                {
                    tag.PropertyChanged += async (sender, args) =>
                    {
                        if (args.PropertyName == nameof(TagResponseModel.IsSelected))
                        {
                            await ApplyFiltersAsync();
                        }
                    };
                }
            }
        };

        _loadTagsCommand.Execute(null);
        _applyFiltersCommand.Execute(null);
    }

    private async Task LoadTagsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var (Success, Message, Data) = await _apiService.GetAllTagsAsync();

            if (Success && Data != null)
            {
                Tags.Clear();
                foreach (var tag in Data)
                {
                    Tags.Add(new TagResponseModel
                    {
                        Id = tag.Id,
                        Name = tag.Name,
                        IsSelected = false,
                    });
                }
            }
            else
            {
                ErrorMessage = Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading tags: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SelectTagByNameAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return;

        if (!Tags.Any())
        {
            await LoadTagsAsync();
        }

        var matchingTag = Tags.FirstOrDefault(t =>
            t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));

        if (matchingTag != null)
        {
            foreach (var tag in Tags.Where(t => t.Id != matchingTag.Id))
            {
                tag.IsSelected = false;
            }
            matchingTag.IsSelected = true;
        }
    }

    private async Task ApplyFiltersAsync()
    {
        try
        {
            // IsLoading = true;
            ErrorMessage = string.Empty;

            var selectedTagIds = Tags
                .Where(t => t.IsSelected)
                .Select(t => t.Id)
                .ToList();

            var request = new EventsRequestModel
            {
                PageNumber = 1,
                PageSize = 50,
                TagIds = selectedTagIds.Any() ? selectedTagIds : null
            };

            (bool Success, string Message, List<EventResponseModel>? Data) = await _apiService.GetEventsAsync(request);

            if (Success && Data != null)
            {
                NothingFound = false;
                Events.Clear();
                if (Data.Count == 0)
                {
                    ErrorMessage = "No events found matching the selected filters.";
                    NothingFound = true;
                }
                else
                {
                    foreach (var evt in Data)
                    {
                        Events.Add(evt);
                    }
                }
            }
            else
            {
                ErrorMessage = Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error applying filters: {ex.Message}";
        }
        /*
        finally
        {
            IsLoading = false;
        }
        */
    }

    [RelayCommand]
    public async Task BuyTicketCommandAsync(EventResponseModel? eventModel)
    {
        if (eventModel == null)
            return;

        var (Success, Message, Data) = await _apiService.GetEventByIdAsync(eventModel.Id, MainPageView.Instance.mainPageViewModel.JwtToken);
        if (!Success || Data == null)
        {
            ErrorMessage = Message;
            return;
        }
        
        var resultData = Data;
        ViewEventView.Instance.viewEventViewModel.InsertFormData(resultData.Title, resultData.Description, resultData.DateTimes[0].DateTime, 
            resultData.ImageUrl, resultData.PlaceAddress, resultData.Price.ToString(), resultData.DateTimes);

        MainPageView.Instance.mainPageViewModel.IsCarouselVisible = false;
        MainPageView.Instance.mainPageViewModel.IsBrowsingEventsAsOrganizer = true;
        MainPageView.Instance.mainPageViewModel.CurrentPage = ViewEventView.Instance;
    }
}