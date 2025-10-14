using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Config;
using Eventa.Converters;
using Eventa.Models.Authentication;
using Eventa.Models.Events;
using Eventa.Services;
using Eventa.Views.Authentication;
using Eventa.Views.Events;
using Eventa.Views.Main;
using System;
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
    private IAsyncRelayCommand? _registerCommand;

    [ObservableProperty]
    private IAsyncRelayCommand _loginCommand;

    [ObservableProperty]
    private IAsyncRelayCommand _logoutCommand;

    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty] 
    private bool _isLoading;
    [ObservableProperty] 
    private string _errorMessage = string.Empty;

    public MainPageViewModel()
    {
        _registerCommand = new AsyncRelayCommand(RegisterAsync);
        _loginCommand = new AsyncRelayCommand(LoginAsync);
        _logoutCommand = new AsyncRelayCommand(LogoutAsync);

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
            await SaveAuthenticationDataAsync(settings, loginResponse.JwtToken);
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

    public void InsertFormData(LoginResponseModel model)
    {
        UserId = model.UserId;
        _ = LoadBrowseTagsAsync();
    }

    [RelayCommand]
    private async Task NavigateToTag(BrowseTagItemModel tag)
    {
        await BrowseEventsView.Instance.browseEventsViewModel.SelectTagByNameAsync(tag.TagName);
    }

    [RelayCommand]
    private void NavigateToMyProfile()
    {
        // TODO: Implement navigation to My Profile page
        // CurrentPage = new MyProfileView();
    }

    [RelayCommand]
    private void NavigateToMyTickets()
    {
        // TODO: Implement navigation to My Tickets page
        // CurrentPage = new MyTicketsView();
    }

    private async Task LoginAsync()
    {
        await ClearAuthenticationDataAsync();
        ResetAllAuthenticationViews();
        MainView.Instance.ChangePage(LoginView.Instance);
    }

    private async Task RegisterAsync()
    {
        await ClearAuthenticationDataAsync();
        ResetAllAuthenticationViews();
        MainView.Instance.ChangePage(RegistrationView.Instance);
    }

    private async Task LogoutAsync()
    {
        await ClearAuthenticationDataAsync();
        ResetAllAuthenticationViews();
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

    private async Task SaveAuthenticationDataAsync(AppSettings settings, string jwtToken)
    {
        settings.JwtToken = jwtToken;
        await _settingsService.SaveAsync(settings);
    }

    private static void ResetAllAuthenticationViews()
    {
        EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
        RegistrationView.Instance.registrationViewModel.ResetForm();
        LoginView.Instance.loginViewModel.ResetForm();
    }

    public void ResetForm()
    {
        UserId = string.Empty;
        BrowseTags.Clear();
    }
}