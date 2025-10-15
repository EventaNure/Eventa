using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Config;
using Eventa.Models.Authentication;
using Eventa.Services;
using Eventa.Views.Authentication;
using Eventa.Views.Main;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Authentication;

public partial class EmailVerifyViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();
    private readonly JsonSettings<AppSettings> _settingsService = new();

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private AsyncRelayCommand? _verifyEmailCommand;

    [ObservableProperty]
    private AsyncRelayCommand? _resendCodeCommand;

    [ObservableProperty]
    private AsyncRelayCommand? _changeEmailCommand;

    private string _userId = string.Empty;
    private string _password = string.Empty;

    public bool IsChangingEmail { get; private set; } = false;

    public EmailVerifyViewModel()
    {
        _verifyEmailCommand = new AsyncRelayCommand(VerifyEmailAsync);
        _resendCodeCommand = new AsyncRelayCommand(ResendCodeAsync);
        _changeEmailCommand = new AsyncRelayCommand(ChangeEmailAsync);
    }

    private async Task VerifyEmailAsync()
    {
        if (!ValidateCode())
            return;

        ErrorMessage = string.Empty;

        try
        {
            var emailConfirmation = new EmailConfirmationRequestModel
            {
                UserId = _userId,
                Code = Code
            };

            var (success, message) = await _apiService.ConfirmEmailAsync(emailConfirmation);

            if (success)
            {
                await AttemptLoginAsync();
            }
            else
            {
                ErrorMessage = ErrorMessageMapper.MapErrorMessage(message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    private async Task ResendCodeAsync()
    {
        ErrorMessage = string.Empty;

        try
        {
            var resendRequest = new ResendEmailConfirmationRequestModel
            {
                UserId = _userId
            };

            var (success, message) = await _apiService.ResendConfirmEmailAsync(resendRequest);

            ErrorMessage = success
                ? "The new code has been sent to your email!"
                : ErrorMessageMapper.MapErrorMessage(message);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    private async Task ChangeEmailAsync()
    {
        var settings = await _settingsService.LoadAsync();
        IsChangingEmail = true;

        RegistrationView.Instance.registrationViewModel.InsertFormData(
            settings.UserName,
            settings.Email,
            settings.Password,
            settings.OrganizationName
        );

        MainView.Instance.ChangePage(RegistrationView.Instance);
    }

    private bool ValidateCode()
    {
        if (Code.Length != 6)
        {
            ErrorMessage = "Please enter a valid 6-digit code.";
            return false;
        }
        return true;
    }

    private async Task AttemptLoginAsync()
    {
        try
        {
            var loginRequest = new LoginRequestModel
            {
                Email = Email,
                Password = _password
            };

            var (success, message, data) = await _apiService.LoginAsync(loginRequest);

            if (success && data is JsonElement json)
            {
                await HandleLoginResponseAsync(json);
            }
            else
            {
                ErrorMessage = ErrorMessageMapper.MapErrorMessage(message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred during login: {ex.Message}";
        }
    }

    private async Task HandleLoginResponseAsync(JsonElement json)
    {
        var loginResponse = json.Deserialize<LoginResponseModel>();

        if (loginResponse == null)
        {
            ErrorMessage = "Failed to process login response.";
            return;
        }

        if (loginResponse.EmailConfirmed)
        {
            NavigateToSuccessPage(loginResponse);
        }

        await Task.CompletedTask;
    }

    private static void NavigateToSuccessPage(LoginResponseModel loginResponse)
    {
        EmailVerifySuccessView.Instance.emailVerifySuccessViewModel.InsertFormData(loginResponse);
        MainView.Instance.ChangePage(EmailVerifySuccessView.Instance);
    }

    public void InsertFormData(string email, string password, string userId)
    {
        IsChangingEmail = false;
        Email = email;
        _password = password;
        _userId = userId;
    }

    public void ResetForm()
    {
        Email = string.Empty;
        Code = string.Empty;
        _password = string.Empty;
        _userId = string.Empty;
        ErrorMessage = string.Empty;
        IsChangingEmail = false;
    }
}