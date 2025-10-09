using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Views;
using Eventa.Views.Authentication;

namespace Eventa.ViewModels.Authentication;

public partial class LoginViewModel : ObservableObject
{
    [RelayCommand]
    private void RegistrationLink()
    {
        MainView.Instance.ChangePage(RegistrationView.Instance);
    }
}