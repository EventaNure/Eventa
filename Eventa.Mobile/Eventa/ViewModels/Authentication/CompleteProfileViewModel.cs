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
    private bool _isGoogleRegistration = false;

    public CompleteProfileViewModel()
    {
        _completeProfileCommand = new AsyncRelayCommand(CompleteProfileAsync);
    }

    public void InsertFormDataFromGoogle(string name, string userId)
    {
        Name = name;
        _userId = userId;
        OrganizationName = string.Empty;
        _isGoogleRegistration = true;
        ErrorMessage = string.Empty;
    }

    private async Task CompleteProfileAsync()
    {
        if (!ValidateInput())
            return;

        ErrorMessage = string.Empty;

        try
        {
            if (!_isGoogleRegistration)
            {
                ErrorMessage = "Invalid registration state";
                return;
            }

            var request = new CompleteExternalRegistrationRequestModel
            {
                Name = Name,
                Organization = string.IsNullOrWhiteSpace(OrganizationName) ? null : OrganizationName,
                UserId = _userId
            };

            var (success, message, data) = await _apiService.CompleteExternalRegistrationAsync(request);

            if (success && data != null)
            {
                await SaveProfileDataAsync(data);
                NavigateToMainPage(data);
            }
            else
            {
                ErrorMessage = ApiErrorConverter.ExtractErrorMessage(message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    private async Task SaveProfileDataAsync(CompleteExternalRegistrationResponseModel response)
    {
        var settings = await _settingsService.LoadAsync();
        settings.UserId = response.UserId;
        settings.JwtToken = response.JwtToken;
        settings.UserName = Name;

        if (!string.IsNullOrWhiteSpace(OrganizationName))
        {
            settings.OrganizationName = OrganizationName;
        }

        await _settingsService.SaveAsync(settings);
    }

    private void NavigateToMainPage(CompleteExternalRegistrationResponseModel response)
    {
        var loginResponse = new LoginResponseModel
        {
            UserId = response.UserId,
            JwtToken = response.JwtToken,
            EmailConfirmed = true
        };

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

        if (!string.IsNullOrWhiteSpace(OrganizationName))
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
        _isGoogleRegistration = false;
    }
}