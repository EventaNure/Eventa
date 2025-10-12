using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Authentication;
using Eventa.Services;
using Eventa.Views;
using Eventa.Views.Authentication;
using Eventa.Views.Main;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Authentication;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private IAsyncRelayCommand? _loginCommand;
    [ObservableProperty]
    private string _email = string.Empty;
    [ObservableProperty]
    private string _password = string.Empty;
    [ObservableProperty]
    private string _errorMessage = string.Empty;


    public LoginViewModel()
    {
        _apiService = new ApiService();
        _loginCommand = new AsyncRelayCommand(Login);
    }

    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required!";
            return;
        }

        ErrorMessage = string.Empty;

        try
        {
            var loginRequestModel = new LoginRequestModel
            {
                Email = Email,
                Password = Password
            };

            // Call API
            var (success, message, data) = await _apiService.LoginAsync(loginRequestModel);

            if (success && data is JsonElement json)
            {
                LoginResponseModel loginResponse = json.Deserialize<LoginResponseModel>()!;
                if (loginResponse.EmailConfirmed)
                {
                    ErrorMessage = "Successfully logged in!";
                    EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
                    RegistrationView.Instance.registrationViewModel.ResetForm();
                    LoginView.Instance.loginViewModel.ResetForm();

                    MainPageView.Instance.mainPageViewModel.InsertFormData(loginResponse);
                    MainView.Instance.ChangePage(MainPageView.Instance);
                }
                else
                {
                    EmailVerifyView.Instance.emailVerifyViewModel.InsertFormData(Email, Password, loginResponse.UserId);
                    MainView.Instance.ChangePage(EmailVerifyView.Instance);
                }
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

    [RelayCommand]
    private void ForgotPasswordLink()
    {
        //MainView.Instance.ChangePage(PasswordResetRequestView.Instance);
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