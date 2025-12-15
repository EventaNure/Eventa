using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Config;
using Eventa.Converters;
using Eventa.Models.Authentication;
using Eventa.Services;
using Eventa.Views.Authentication;
using Eventa.Views.Main;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Authentication;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();
    private readonly JsonSettings<AppSettings> _settingsService = new();

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _organizationName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private AsyncRelayCommand? _registerCommand;

    [ObservableProperty]
    private AsyncRelayCommand? _googleRegisterCommand;

    public RegistrationViewModel()
    {
        _registerCommand = new AsyncRelayCommand(RegisterAsync);
        _googleRegisterCommand = new AsyncRelayCommand(GoogleRegisterAsync);
    }

    private async Task RegisterAsync()
    {
        if (!ValidateInput())
            return;

        ErrorMessage = string.Empty;

        try
        {
            if (EmailVerifyView.Instance.emailVerifyViewModel.IsChangingEmail)
            {
                await HandleEmailChangeAsync();
            }
            else
            {
                await HandleNewRegistrationAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    private async Task GoogleRegisterAsync()
    {
        ErrorMessage = string.Empty;

        try
        {
            // Initialize Google Auth Service
            var googleAuthService = new GoogleAuthService();

            // Authenticate with Google and get ID token
            string? idToken = await googleAuthService.AuthenticateAsync();

            if (string.IsNullOrEmpty(idToken))
            {
                ErrorMessage = "Failed to authenticate with Google. Please try again.";
                return;
            }

            var googleRequest = new GoogleLoginRequestModel
            {
                IdToken = idToken
            };

            // Get user info from Google
            var (success, message, data) = await _apiService.GoogleUserLoginAsync(googleRequest);

            if (success && data != null)
            {
                await HandleGoogleRegistrationResponseAsync(data);
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

    private async Task HandleGoogleRegistrationResponseAsync(GoogleLoginResponseModel response)
    {
        if (response.IsLogin)
        {
            var loginResponse = new LoginResponseModel
            {
                UserId = response.UserId,
                JwtToken = response.JwtToken!,
                EmailConfirmed = true
            };

            // Save only JWT token and user info (no Google ID token or credentials)
            await SaveGoogleCredentialsAsync(loginResponse, response.Name);
            ResetAllAuthenticationViews();
            MainPageView.Instance.mainPageViewModel.InsertFormData(loginResponse);
            MainView.Instance.ChangePage(MainPageView.Instance);
        }
        else
        {
            CompleteProfileView.Instance.completeProfileViewModel.InsertFormDataFromGoogle(
                response.Name,
                response.UserId
            );
            MainView.Instance.ChangePage(CompleteProfileView.Instance);
        }
    }

    private async Task SaveGoogleCredentialsAsync(LoginResponseModel loginResponse, string userName)
    {
        var settings = await _settingsService.LoadAsync();
        settings.JwtToken = loginResponse.JwtToken;
        settings.UserId = loginResponse.UserId;
        settings.UserName = userName;
        settings.Email = string.Empty;
        settings.Password = string.Empty;
        await _settingsService.SaveAsync(settings);
    }

    private async Task HandleEmailChangeAsync()
    {
        var loginRequest = new LoginRequestModel
        {
            Email = Email,
            Password = Password
        };

        var (success, message, _) = await _apiService.LoginAsync(loginRequest);
        if (success)
        {
            var settings = await _settingsService.LoadAsync();
            EmailVerifyView.Instance.emailVerifyViewModel.InsertFormData(Email, Password, settings.UserId);
            MainView.Instance.ChangePage(EmailVerifyView.Instance);
        }
        else
        {
            ErrorMessage = ApiErrorConverter.ExtractErrorMessage(message);
        }
    }

    private async Task HandleNewRegistrationAsync()
    {
        var registerRequest = new RegisterRequestModel
        {
            UserName = UserName,
            Email = Email,
            Password = Password,
            ConfirmPassword = ConfirmPassword,
            OrganizationName = string.IsNullOrWhiteSpace(OrganizationName) ? null : OrganizationName
        };

        var (success, message, data) = await _apiService.RegisterAsync(registerRequest);

        if (success && data is JsonElement json)
        {
            await SaveRegistrationDataAsync(json);
            NavigateToEmailVerification(json.GetProperty("userId").GetString() ?? string.Empty);
        }
        else
        {
            ErrorMessage = ApiErrorConverter.ExtractErrorMessage(message);
        }
    }

    private async Task SaveRegistrationDataAsync(JsonElement json)
    {
        string userId = json.GetProperty("userId").GetString() ?? string.Empty;

        var settings = await _settingsService.LoadAsync();
        settings.UserId = userId;
        settings.Email = Email;
        settings.Password = Password;
        settings.UserName = UserName;
        settings.OrganizationName = OrganizationName;
        await _settingsService.SaveAsync(settings);
    }

    private void NavigateToEmailVerification(string userId)
    {
        EmailVerifyView.Instance.emailVerifyViewModel.InsertFormData(Email, Password, userId);
        MainView.Instance.ChangePage(EmailVerifyView.Instance);
    }

    private static void ResetAllAuthenticationViews()
    {
        EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
        RegistrationView.Instance.registrationViewModel.ResetForm();
        LoginView.Instance.loginViewModel.ResetForm();
        CompleteProfileView.Instance.completeProfileViewModel.ResetForm();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(UserName) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "All fields are required!";
            return false;
        }

        if (ConfirmPassword != Password)
        {
            ErrorMessage = "Passwords must match!";
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void LoginLink()
    {
        MainView.Instance.ChangePage(LoginView.Instance);
    }

    [RelayCommand]
    private void BackToMain()
    {
        MainView.Instance.ChangePage(MainPageView.Instance);
    }

    public void InsertFormData(string userName, string email, string password, string? organizationName)
    {
        UserName = userName;
        Email = email;
        Password = password;
        ConfirmPassword = password;
        OrganizationName = organizationName ?? string.Empty;
    }

    public void ResetForm()
    {
        UserName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        OrganizationName = string.Empty;
        ErrorMessage = string.Empty;
    }
}