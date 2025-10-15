using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Config;
using Eventa.Models.Authentication;
using Eventa.Views.Authentication;
using Eventa.Views.Main;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Authentication;

public partial class EmailVerifySuccessViewModel : ObservableObject
{
    private readonly JsonSettings<AppSettings> _settingsService = new();

    [ObservableProperty]
    private AsyncRelayCommand? _goToLoginCommand;

    private LoginResponseModel _loginResponseModel = new();

    public EmailVerifySuccessViewModel()
    {
        _goToLoginCommand = new AsyncRelayCommand(GoToMainPageAsync);
    }

    private async Task GoToMainPageAsync()
    {
        ResetAllAuthenticationViews();
        await SaveAuthenticationTokenAsync();
        NavigateToMainPage();
    }

    private static void ResetAllAuthenticationViews()
    {
        EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
        RegistrationView.Instance.registrationViewModel.ResetForm();
        LoginView.Instance.loginViewModel.ResetForm();
    }

    private async Task SaveAuthenticationTokenAsync()
    {
        var settings = await _settingsService.LoadAsync();
        settings.JwtToken = _loginResponseModel.JwtToken;
        await _settingsService.SaveAsync(settings);
    }

    private void NavigateToMainPage()
    {
        MainPageView.Instance.mainPageViewModel.InsertFormData(_loginResponseModel);
        MainView.Instance.ChangePage(MainPageView.Instance);
    }

    public void InsertFormData(LoginResponseModel model)
    {
        _loginResponseModel = model;
    }
}