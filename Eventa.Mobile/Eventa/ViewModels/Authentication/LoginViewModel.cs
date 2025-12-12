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

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();
    private readonly JsonSettings<AppSettings> _settingsService = new();

    [ObservableProperty]
    private AsyncRelayCommand? _loginCommand;

    [ObservableProperty]
    private AsyncRelayCommand? _googleLoginCommand;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel()
    {
        _loginCommand = new AsyncRelayCommand(LoginAsync);
        _googleLoginCommand = new AsyncRelayCommand(GoogleLoginAsync);
    }

    private async Task LoginAsync()
    {
        if (!ValidateInput())
            return;

        ErrorMessage = string.Empty;

        try
        {
            var loginRequest = new LoginRequestModel
            {
                Email = Email,
                Password = Password
            };

            var (success, message, data) = await _apiService.LoginAsync(loginRequest);

            if (success && data is JsonElement json)
            {
                await HandleSuccessfulLoginAsync(json);
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

    private async Task GoogleLoginAsync()
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

            // Send token to backend for verification
            var googleRequest = new GoogleLoginRequestModel
            {
                IdToken = idToken
            };

            var (success, message, data) = await _apiService.GoogleUserLoginAsync(googleRequest);

            if (success && data != null)
            {
                await HandleGoogleLoginResponseAsync(data);
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

    private async Task HandleGoogleLoginResponseAsync(GoogleLoginResponseModel response)
    {
        if (response.IsLogin)
        {
            // User is already registered and has a complete profile
            var loginResponse = new LoginResponseModel
            {
                UserId = response.UserId,
                JwtToken = response.JwtToken!,
                EmailConfirmed = true
            };

            await SaveGoogleCredentialsAsync(loginResponse, response.Name);
            ResetAllAuthenticationViews();
            MainPageView.Instance.mainPageViewModel.InsertFormData(loginResponse);
            MainView.Instance.ChangePage(MainPageView.Instance);
        }
        else
        {
            // User needs to complete registration
            // Show complete profile view to finish account setup
            CompleteProfileView.Instance.completeProfileViewModel.InsertFormDataFromGoogle(
                response.Name,
                response.UserId
            );
            MainView.Instance.ChangePage(CompleteProfileView.Instance);
        }
    }

    private async Task HandleSuccessfulLoginAsync(JsonElement json)
    {
        var loginResponse = json.Deserialize<LoginResponseModel>()!;

        if (loginResponse.EmailConfirmed)
        {
            await HandleConfirmedEmailAsync(loginResponse);
        }
        else
        {
            NavigateToEmailVerification(loginResponse.UserId);
        }
    }

    private async Task HandleConfirmedEmailAsync(LoginResponseModel loginResponse)
    {
        ErrorMessage = "Successfully logged in!";

        await SaveCredentialsAsync(loginResponse);

        ResetAllAuthenticationViews();
        MainPageView.Instance.mainPageViewModel.InsertFormData(loginResponse);
        MainView.Instance.ChangePage(MainPageView.Instance);
    }

    private async Task SaveCredentialsAsync(LoginResponseModel loginResponse)
    {
        var settings = await _settingsService.LoadAsync();
        settings.Email = Email;
        settings.Password = Password;
        settings.JwtToken = loginResponse.JwtToken;
        settings.UserId = loginResponse.UserId;
        await _settingsService.SaveAsync(settings);
    }

    private async Task SaveGoogleCredentialsAsync(LoginResponseModel loginResponse, string userName)
    {
        var settings = await _settingsService.LoadAsync();
        settings.JwtToken = loginResponse.JwtToken;
        settings.UserId = loginResponse.UserId;
        settings.UserName = userName;
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
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required!";
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void ForgotPasswordLink()
    {
        //MainView.Instance.ChangePage(PasswordResetRequestView.Instance);
    }

    [RelayCommand]
    private void BackToMain()
    {
        MainView.Instance.ChangePage(MainPageView.Instance);
    }

    [RelayCommand]
    private void RegistrationLink()
    {
        MainView.Instance.ChangePage(RegistrationView.Instance);
    }

    public void ResetForm()
    {
        Email = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }
}