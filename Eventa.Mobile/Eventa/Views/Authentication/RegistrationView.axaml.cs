using Avalonia.Controls;
using Eventa.ViewModels.Authentication;

namespace Eventa.Views.Authentication;

public partial class RegistrationView : UserControl
{
    private static RegistrationView? _instance;
    public static RegistrationView Instance
    {
        get
        {
            _instance ??= new RegistrationView();
            return _instance;
        }
    }

    public readonly RegistrationViewModel registrationViewModel = new();

    public RegistrationView()
    {
        DataContext = registrationViewModel;
        InitializeComponent();
    }
}