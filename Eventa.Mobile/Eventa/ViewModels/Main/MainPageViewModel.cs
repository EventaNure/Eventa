using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Authentication;
using Eventa.Views;
using Eventa.Views.Authentication;
using Eventa.Views.Main;

namespace Eventa.ViewModels.Main;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userId = string.Empty;

    public void InsertFormData(LoginResponseModel model)
    {
        UserId = model.UserId;
    }

    [RelayCommand]
    private void Logout()
    {
        UserId = string.Empty;
        ResetForm();
        EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
        RegistrationView.Instance.registrationViewModel.ResetForm();
        LoginView.Instance.loginViewModel.ResetForm();
        MainView.Instance.ChangePage(LoginView.Instance);
    }

    public void ResetForm()
    {
        UserId = string.Empty;
    }
}