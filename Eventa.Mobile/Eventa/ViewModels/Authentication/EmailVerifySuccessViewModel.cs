using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Views;
using Eventa.Views.Authentication;

namespace Eventa.ViewModels.Authentication;

public partial class EmailVerifySuccessViewModel : ObservableObject
{
    [RelayCommand]
    private void GoToLogin()
    {
        EmailVerifyView.Instance.emailVerifyViewModel.ResetForm();
        RegistrationView.Instance.registrationViewModel.ResetForm();
        LoginView.Instance.loginViewModel.ResetForm();
        MainView.Instance.ChangePage(LoginView.Instance);
    }
}