using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models;
using Eventa.Services;
using Eventa.Views;
using Eventa.Views.Authentication;
using System;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Authentication;

public partial class EmailVerifyViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string _email = string.Empty;
    [ObservableProperty]
    private string _code = string.Empty;
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private IAsyncRelayCommand? _verifyEmailCommand;
    [ObservableProperty]
    private IAsyncRelayCommand? _resendCodeCommand;

    private string UserId = string.Empty;

    public EmailVerifyViewModel()
    {
        _verifyEmailCommand = new AsyncRelayCommand(VerifyEmail);
        _resendCodeCommand = new AsyncRelayCommand(ResendCode);
        _apiService = new ApiService();
    }

    private async Task VerifyEmail()
    {
        if (Code.Length < 6)
        {
            ErrorMessage = "Please enter a valid 6-digit code.";
            return;
        }

        try
        {
            var emailConfirmationRequestModel = new EmailConfirmationRequestModel
            {
                UserId = UserId,
                Code = Code
            };

            // Call API
            var (success, message) = await _apiService.ConfirmEmailAsync(emailConfirmationRequestModel);

            if (success)
            {
                // EmailVerifyView.Instance.emailVerifyViewModel.InsertFormData(Email);
                MainView.Instance.ChangePage(EmailVerifySuccessView.Instance);
            }
            else
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }

    private async Task ResendCode()
    {
        ErrorMessage = "In development";
        await Task.Delay(1000);
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void ChangeEmail()
    {
        MainView.Instance.ChangePage(RegistrationView.Instance);
    }

    public void InsertFormData(string email, string userId)
    {
        Email = email;
        UserId = userId;
    }

    public void ResetForm()
    {
        Email = string.Empty;
        Code = string.Empty;
        ErrorMessage = string.Empty;
        UserId = string.Empty;
    }
}