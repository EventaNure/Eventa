using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Authentication;
using Eventa.Views;
using Eventa.Views.Authentication;
using Eventa.Views.Main;

namespace Eventa.ViewModels.Authentication;

public partial class EmailVerifySuccessViewModel : ObservableObject
{
    private LoginResponseModel _loginResponseModel = new();

    [RelayCommand]
    private void GoToLogin()
    {
        EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
        RegistrationView.Instance.registrationViewModel.ResetForm();
        LoginView.Instance.loginViewModel.ResetForm();
        MainPageView.Instance.mainPageViewModel.InsertFormData(_loginResponseModel);
        MainView.Instance.ChangePage(MainPageView.Instance);
    }

    public void InsertFormData(LoginResponseModel model)
    {
        _loginResponseModel = model;
    }
}