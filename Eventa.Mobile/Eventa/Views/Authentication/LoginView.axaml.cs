using Avalonia.Controls;
using Eventa.ViewModels.Authentication;

namespace Eventa.Views.Authentication;

public partial class LoginView : UserControl
{
    private static LoginView? _instance;
    public static LoginView Instance
    {
        get
        {
            _instance ??= new LoginView();
            return _instance;
        }
    }

    public readonly LoginViewModel loginViewModel = new();

    public LoginView()
    {
        DataContext = loginViewModel;
        InitializeComponent();
    }
}