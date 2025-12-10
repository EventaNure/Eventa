using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Config;
using Eventa.Converters;
using Eventa.Models.Authentication;
using Eventa.Services;
using Eventa.Views.Main;
using System;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Authentication;

public partial class CompleteProfileViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();
    private readonly JsonSettings<AppSettings> _settingsService = new();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _organizationName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private AsyncRelayCommand? _completeProfileCommand;

    private string _userId = string.Empty;
    private string _jwtToken = string.Empty;
    private string _googleIdToken = string.Empty;
    private bool _isGoogleLogin = false;

    public CompleteProfileViewModel()
    {
        _completeProfileCommand = new AsyncRelayCommand(CompleteProfileAsync);
    }

    public void InsertFormData(string name, string userId, string jwtToken)
    {
        Name = name;
        _userId = userId;
        _jwtToken = jwtToken;
        OrganizationName = string.Empty;
        _isGoogleLogin = false;
        _googleIdToken = string.Empty;
    }

    public void InsertFormDataFromGoogle(string name, string userId, string jwtToken, string googleIdToken)
    {
        Name = name;
        _userId = userId;
        _jwtToken = jwtToken;
        _googleIdToken = googleIdToken;
        _isGoogleLogin = true;
        OrganizationName = string.Empty;
    }

    private async Task CompleteProfileAsync()
    {
        if (!ValidateInput())
            return;

        ErrorMessage = string.Empty;

        try
        {
            bool isOrganizer = !string.IsNullOrWhiteSpace(OrganizationName);

            if (_isGoogleLogin)
            {
                // For Google login, call the appropriate API based on organization name
                await HandleGoogleProfileCompletionAsync(isOrganizer);
            }
            else
            {
                // For regular registration, update profile using JWT token
                await HandleRegularProfileCompletionAsync(isOrganizer);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    private async Task HandleGoogleProfileCompletionAsync(bool isOrganizer)
    {
        var googleRequest = new GoogleLoginRequestModel
        {
            IdToken = _googleIdToken
        };

        bool success;
        string message;
        GoogleLoginResponseModel? data;

        if (isOrganizer)
        {
            (success, message, data) = await _apiService.GoogleOrganizerLoginAsync(googleRequest);
        }
        else
        {
            (success, message, data) = await _apiService.GoogleUserLoginAsync(googleRequest);
        }

        if (success && data != null)
        {
            // Now save the profile data to the database
            bool profileUpdateSuccess;
            string profileUpdateMessage;

            if (isOrganizer)
            {
                var request = new PersonalOrganizerDataRequestModel
                {
                    Name = Name,
                    Organization = OrganizationName
                };
                (profileUpdateSuccess, profileUpdateMessage) = await _apiService.UpdateOrganizerProfileAsync(request, data.JwtToken);
            }
            else
            {
                var request = new PersonalUserDataRequestModel
                {
                    Name = Name
                };
                (profileUpdateSuccess, profileUpdateMessage) = await _apiService.UpdateUserProfileAsync(request, data.JwtToken);
            }

            if (profileUpdateSuccess)
            {
                await SaveGoogleProfileDataAsync(data);
                NavigateToMainPage(data);
            }
            else
            {
                ErrorMessage = ApiErrorConverter.ExtractErrorMessage(profileUpdateMessage);
            }
        }
        else
        {
            ErrorMessage = ApiErrorConverter.ExtractErrorMessage(message);
        }
    }

    private async Task HandleRegularProfileCompletionAsync(bool isOrganizer)
    {
        bool success;
        string message;

        if (isOrganizer)
        {
            var request = new PersonalOrganizerDataRequestModel
            {
                Name = Name,
                Organization = OrganizationName
            };
            (success, message) = await _apiService.UpdateOrganizerProfileAsync(request, _jwtToken);
        }
        else
        {
            var request = new PersonalUserDataRequestModel
            {
                Name = Name
            };
            (success, message) = await _apiService.UpdateUserProfileAsync(request, _jwtToken);
        }

        if (success)
        {
            await SaveProfileDataAsync();
            NavigateToMainPage();
        }
        else
        {
            ErrorMessage = ApiErrorConverter.ExtractErrorMessage(message);
        }
    }

    private async Task SaveGoogleProfileDataAsync(GoogleLoginResponseModel response)
    {
        var settings = await _settingsService.LoadAsync();
        settings.UserId = response.UserId;
        settings.JwtToken = response.JwtToken;
        settings.UserName = response.Name;

        bool isOrganizer = !string.IsNullOrWhiteSpace(OrganizationName);
        if (isOrganizer)
        {
            settings.OrganizationName = OrganizationName;
        }

        await _settingsService.SaveAsync(settings);
    }

    private async Task SaveProfileDataAsync()
    {
        var settings = await _settingsService.LoadAsync();
        settings.UserId = _userId;
        settings.JwtToken = _jwtToken;
        settings.UserName = Name;

        bool isOrganizer = !string.IsNullOrWhiteSpace(OrganizationName);
        if (isOrganizer)
        {
            settings.OrganizationName = OrganizationName;
        }

        await _settingsService.SaveAsync(settings);
    }

    private void NavigateToMainPage(GoogleLoginResponseModel? response = null)
    {
        LoginResponseModel loginResponse;

        if (response != null)
        {
            loginResponse = new LoginResponseModel
            {
                UserId = response.UserId,
                JwtToken = response.JwtToken,
                EmailConfirmed = true
            };
        }
        else
        {
            loginResponse = new LoginResponseModel
            {
                UserId = _userId,
                JwtToken = _jwtToken,
                EmailConfirmed = true
            };
        }

        MainPageView.Instance.mainPageViewModel.InsertFormData(loginResponse);
        MainView.Instance.ChangePage(MainPageView.Instance);
        ResetForm();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required!";
            return false;
        }

        if (Name.Length < 3 || Name.Length > 32)
        {
            ErrorMessage = "Name must be between 3 and 32 characters!";
            return false;
        }

        bool isOrganizer = !string.IsNullOrWhiteSpace(OrganizationName);
        if (isOrganizer)
        {
            if (OrganizationName.Length < 3 || OrganizationName.Length > 32)
            {
                ErrorMessage = "Organization name must be between 3 and 32 characters!";
                return false;
            }
        }

        return true;
    }

    public void ResetForm()
    {
        Name = string.Empty;
        OrganizationName = string.Empty;
        ErrorMessage = string.Empty;
        _userId = string.Empty;
        _jwtToken = string.Empty;
        _googleIdToken = string.Empty;
        _isGoogleLogin = false;
    }
}