using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Config;
using Eventa.Controls;
using Eventa.Converters;
using Eventa.Models.Authentication;
using Eventa.Models.Events;
using Eventa.Models.Events.Organizer;
using Eventa.Services;
using Eventa.Views.Authentication;
using Eventa.Views.Events;
using Eventa.Views.Main;
using Eventa.Views.Ordering;
using Eventa.Views.Tickets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Main;

public partial class MainPageViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();
    private readonly JsonSettings<AppSettings> _settingsService = new();

    [ObservableProperty]
    private ObservableCollection<BrowseTagItemModel> _browseTags = [];

    [ObservableProperty]
    private UserControl? _currentPage;

    [ObservableProperty]
    private UserControl? _organizerEvents;

    [ObservableProperty]
    private List<string> imageUrls = [
        "https://gfmoritracker.fun/gallery/ghostFace.webp",
        "https://gfmoritracker.fun/gallery/portraits/dwight_fairfield.webp",
        "https://gfmoritracker.fun/gallery/portraits/meg_thomas.webp",
        "https://gfmoritracker.fun/gallery/portraits/claudette_morel.webp",
        "https://gfmoritracker.fun/gallery/portraits/jake_park.webp",
        "https://gfmoritracker.fun/gallery/portraits/nea_karlsson.webp",
        "https://gfmoritracker.fun/gallery/portraits/laurie_strode.webp",
        "https://gfmoritracker.fun/gallery/portraits/ace_visconti.webp",
        "https://gfmoritracker.fun/gallery/portraits/bill_overbeck.webp",
        "https://gfmoritracker.fun/gallery/portraits/feng_min.webp",
        "https://gfmoritracker.fun/gallery/portraits/david_king.webp",
        "https://gfmoritracker.fun/gallery/portraits/kate_denson.webp",
    ];

    [ObservableProperty]
    private AsyncRelayCommand? _registerCommand;

    [ObservableProperty]
    private AsyncRelayCommand _loginCommand;

    [ObservableProperty]
    private AsyncRelayCommand _logoutCommand;

    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isUserBrowsing;

    [ObservableProperty]
    private bool _isCarouselVisible;

    [ObservableProperty]
    private bool _isBrowsingOrganizerEvents;

    [ObservableProperty]
    private bool _isBrowsingEventsAsOrganizer;

    [ObservableProperty]
    private bool _isOrganizer;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _jwtToken = "";

    public MainPageViewModel()
    {
        _registerCommand = new AsyncRelayCommand(RegisterAsync);
        _loginCommand = new AsyncRelayCommand(LoginAsync);
        _logoutCommand = new AsyncRelayCommand(LogoutAsync);
        _currentPage = BrowseEventsView.Instance;
        _organizerEvents = BrowseOrganizerEventsView.Instance;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadBrowseTagsAsync();
        await LoadProfileAsync();
    }

    public async Task LoadBrowseTagsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        BrowseTags.Clear();

        try
        {
            var (success, message, data) = await _apiService.GetMainTagsAsync();

            if (success && data is JsonArray json)
            {
                PopulateBrowseTags(json);
            }
            else
            {
                AddErrorTag(message);
                ErrorMessage = $"Failed to fetch tags: {ApiErrorConverter.ExtractErrorMessage(message)}";
            }
        }
        catch (Exception ex)
        {
            AddErrorTag(ex.Message);
            ErrorMessage = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void PopulateBrowseTags(JsonArray json)
    {
        BrowseTags.Clear();
        foreach (var element in json)
        {
            if (element is not JsonObject obj) continue;

            BrowseTags.Add(new BrowseTagItemModel
            {
                TagId = obj["id"]?.GetValue<int>() ?? -1,
                TagName = obj["name"]?.GetValue<string>() ?? string.Empty
            });
        }

        if (BrowseTags.Count > 0)
        {
            BrowseTags[^1].IsLastItem = true;
        }
    }

    private void AddErrorTag(string message)
    {
        var errorMessage = ApiErrorConverter.ExtractErrorMessage(message);
        BrowseTags.Add(new BrowseTagItemModel
        {
            TagName = $"Failed to fetch tags, try again... (Error: {errorMessage})",
            IsError = true,
            IsLastItem = true
        });
    }

    private async Task LoadProfileAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var settings = await _settingsService.LoadAsync();

            if (string.IsNullOrEmpty(settings.Email) || string.IsNullOrEmpty(settings.Password))
            {
                UserId = string.Empty;
                return;
            }

            var loginRequest = new LoginRequestModel
            {
                Email = settings.Email,
                Password = settings.Password
            };

            var (success, message, data) = await _apiService.LoginAsync(loginRequest);

            if (success && data is JsonElement json)
            {
                await HandleSuccessfulLoginAsync(json, settings);
            }
            else
            {
                UserId = string.Empty;
                ErrorMessage = $"Profile load failed: {ApiErrorConverter.ExtractErrorMessage(message)}";
            }
        }
        catch (Exception ex)
        {
            UserId = string.Empty;
            ErrorMessage = $"Error loading profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSuccessfulLoginAsync(JsonElement json, AppSettings settings)
    {
        var loginResponse = json.Deserialize<LoginResponseModel>()!;

        if (loginResponse.EmailConfirmed)
        {
            await SaveAuthenticationDataAsync(settings, loginResponse);

            ResetAllAuthenticationViews();
            InsertFormData(loginResponse);
            MainView.Instance.ChangePage(MainPageView.Instance);
        }
        else
        {
            EmailVerifyView.Instance.emailVerifyViewModel.InsertFormData(
                settings.Email,
                settings.Password,
                loginResponse.UserId
            );
            MainView.Instance.ChangePage(EmailVerifyView.Instance);
        }
    }

    private async Task SaveAuthenticationDataAsync(AppSettings settings, LoginResponseModel loginResponse)
    {
        settings.JwtToken = loginResponse.JwtToken;
        settings.UserId = loginResponse.UserId;
        await _settingsService.SaveAsync(settings);
    }

    public void InsertFormData(LoginResponseModel model)
    {
        UserId = model.UserId;
        JwtToken = model.JwtToken;
        _ = LoadBrowseTagsAsync();
        _ = LoadOrganizerEventsAsync(model.JwtToken);
    }

    public async Task LoadOrganizerEventsAsync(string jwtToken)
    {
        var (success, _, data) = await _apiService.GetOrganizerEventsAsync(jwtToken);

        if (success)
        {
            SetupOrganizerMode(data);
        }
        else
        {
            SetupNonOrganizerMode();
        }
    }

    private void SetupOrganizerMode(List<OrganizerEventResponseModel>? events)
    {
        IsOrganizer = true;
        IsBrowsingOrganizerEvents = false;
        IsBrowsingEventsAsOrganizer = false;

        ResetPages();

        var organizerEventsViewModel = BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel;
        organizerEventsViewModel.Events.Clear();
        organizerEventsViewModel.IsCompact = true;
        organizerEventsViewModel.NoEvents = events == null || events.Count == 0;

        if (events != null)
        {
            foreach (var eventModel in events)
            {
                organizerEventsViewModel.Events.Add(eventModel);
            }
        }
    }

    private void SetupNonOrganizerMode()
    {
        IsOrganizer = false;
        IsBrowsingOrganizerEvents = false;
        IsBrowsingEventsAsOrganizer = false;

        ResetPages();

        var organizerEventsViewModel = BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel;
        organizerEventsViewModel.Events.Clear();
        organizerEventsViewModel.IsCompact = true;
    }

    private void ResetPages()
    {
        CurrentPage = null;
        OrganizerEvents = null;
        CurrentPage = BrowseEventsView.Instance;
        OrganizerEvents = BrowseOrganizerEventsView.Instance;

        ViewEventView.Instance.viewEventViewModel.ClearFormData();
        SeatOrderView.Instance.seatOrderViewModel.ClearFormData();
        TicketOrderView.Instance.ticketOrderViewModel.ClearFormData();
        ViewPurchasedTicketsView.Instance.viewPurchasedTicketsViewModel.ClearFormData();
    }

    [RelayCommand]
    private async Task NavigateToTag(BrowseTagItemModel tag)
    {
        IsUserBrowsing = true;
        IsBrowsingOrganizerEvents = false;
        IsBrowsingEventsAsOrganizer = true;

        ViewEventView.Instance.viewEventViewModel.ClearFormData();
        BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCompact = true;
        await BrowseEventsView.Instance.browseEventsViewModel.SelectTagByNameAsync(tag.TagName);

        ResetPages();
    }

    [RelayCommand]
    private void NavigateToMyProfile()
    {
        // TODO: Implement navigation to My Profile page
        // CurrentPage = new MyProfileView();
    }

    [RelayCommand]
    private async Task NavigateToMyTickets()
    {
        await ViewPurchasedTicketsView.Instance.viewPurchasedTicketsViewModel.LoadTicketsAsync();
        MainPageView.Instance.mainPageViewModel.IsCarouselVisible = false;
        MainPageView.Instance.mainPageViewModel.IsBrowsingEventsAsOrganizer = true;
        MainPageView.Instance.mainPageViewModel.CurrentPage = ViewPurchasedTicketsView.Instance;
    }

    [RelayCommand]
    private async Task NavigateToMyCart()
    {
        string jwtToken = MainPageView.Instance.mainPageViewModel.JwtToken;

        if (string.IsNullOrEmpty(jwtToken))
        {
            await DialogControl.Instance.Show("Not Logged In", "You must be logged in to view your cart.", "OK");
            return;
        }

        var (success, message, cartData) = await _apiService.GetTicketsInCartByUserAsync(jwtToken);

        if (!success)
        {
            await DialogControl.Instance.Show("Cart Error", message, "OK");
            return;
        }

        if (cartData == null || cartData.Tickets.Count == 0)
        {
            await DialogControl.Instance.Show("Empty Cart", "You have no tickets in your cart.", "OK");
            return;
        }

        int eventDateTimeId = cartData.EventDateTimeId;

        var (hallSuccess, hallMessage, hallData) = await _apiService.GetFreeSeatsWithHallPlanAsync(eventDateTimeId, jwtToken);
        if (!hallSuccess || hallData == null)
        {
            await DialogControl.Instance.Show("Load Failed", hallMessage, "OK");
            return;
        }

        var settings = await _settingsService.LoadAsync();

        string eventName = cartData.EventName ?? "My Event";

        DateTime dateTime = DateTime.Now;
        if (DateTime.TryParse(settings.CartDateTime, out DateTime result))
        {
            dateTime = result;
        }

        var eventDateTimes = new EventDateTimes
        {
            Id = eventDateTimeId,
            DateTime = dateTime
        };

        var seatOrderVm = SeatOrderView.Instance.seatOrderViewModel;

        await seatOrderVm.InsertFormData(hallData, eventName, eventDateTimes, jwtToken);

        MainPageView.Instance.mainPageViewModel.IsCarouselVisible = false;
        MainPageView.Instance.mainPageViewModel.IsBrowsingEventsAsOrganizer = true;
        MainPageView.Instance.mainPageViewModel.CurrentPage = SeatOrderView.Instance;
    }

    [RelayCommand]
    private void HeaderTitleClicked()
    {
        IsUserBrowsing = false;
        IsBrowsingOrganizerEvents = false;
        IsBrowsingEventsAsOrganizer = false;

        BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCompact = true;
        CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.CancelCommand.Execute(null);

        ResetPages();
    }

    [RelayCommand]
    private void OrganizerEventsClicked()
    {
        IsUserBrowsing = false;
        IsBrowsingOrganizerEvents = true;
        IsBrowsingEventsAsOrganizer = false;

        BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCompact = false;

        CurrentPage = null;
        OrganizerEvents = null;
        CurrentPage = BrowseOrganizerEventsView.Instance;
    }

    private async Task LoginAsync()
    {
        await ClearAuthenticationDataAsync();
        ResetAllAuthenticationViews();
        await LoadBrowseTagsAsync();
        MainView.Instance.ChangePage(LoginView.Instance);
    }

    private async Task RegisterAsync()
    {
        await ClearAuthenticationDataAsync();
        ResetAllAuthenticationViews();
        await LoadBrowseTagsAsync();
        MainView.Instance.ChangePage(RegistrationView.Instance);
    }

    private async Task LogoutAsync()
    {
        await ClearAuthenticationDataAsync();
        ClearOrganizerState();
        ResetAllAuthenticationViews();
        IsUserBrowsing = false;
        IsBrowsingOrganizerEvents = false;
        IsBrowsingEventsAsOrganizer = false;
        BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCompact = true;
        BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel.IsCreating = false;
        CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.ClearForm();
        ResetPages();
        await LoadBrowseTagsAsync();
        MainView.Instance.ChangePage(MainPageView.Instance);
    }

    private async Task ClearAuthenticationDataAsync()
    {
        UserId = string.Empty;

        var settings = await _settingsService.LoadAsync();
        settings.JwtToken = string.Empty;
        settings.Email = string.Empty;
        settings.Password = string.Empty;
        settings.UserName = string.Empty;
        settings.OrganizationName = string.Empty;
        settings.UserId = string.Empty;
        await _settingsService.SaveAsync(settings);

        ResetForm();
    }

    private void ClearOrganizerState()
    {
        IsOrganizer = false;
        IsBrowsingOrganizerEvents = false;
        IsBrowsingEventsAsOrganizer = false;

        OrganizerEvents = null;

        var organizerEventsViewModel = BrowseOrganizerEventsView.Instance.browseOrganizerEventsViewModel;
        organizerEventsViewModel.Events.Clear();
        organizerEventsViewModel.IsCompact = true;
        organizerEventsViewModel.NoEvents = false;
    }

    private static void ResetAllAuthenticationViews()
    {
        EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
        RegistrationView.Instance.registrationViewModel.ResetForm();
        LoginView.Instance.loginViewModel.ResetForm();
    }

    public async void ResetForm()
    {
        UserId = string.Empty;
        await LoadBrowseTagsAsync();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        IsCarouselVisible = !IsUserBrowsing && !value;
    }

    partial void OnIsUserBrowsingChanged(bool value)
    {
        IsCarouselVisible = !value && !IsLoading;
    }
}