using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Authentication;
using Eventa.Services;
using Eventa.Views;
using Eventa.Views.Authentication;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Authentication;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly ApiService _apiService;

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
    private IAsyncRelayCommand? _registerCommand;

    public RegistrationViewModel()
    {
        _apiService = new ApiService();
        _registerCommand = new AsyncRelayCommand(RegisterAsync);
    }

    private async Task RegisterAsync()
    {
        if (!ValidateInput())
            return;

        ErrorMessage = string.Empty;

        try
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
                string userId = json.GetProperty("userId").GetString() ?? string.Empty;
                EmailVerifyView.Instance.emailVerifyViewModel.InsertFormData(Email, Password, userId);
                MainView.Instance.ChangePage(EmailVerifyView.Instance);
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