using Avalonia.Controls;
using Eventa.ViewModels.Authentication;

namespace Eventa.Views.Authentication;

public partial class EmailVerifyView : UserControl
{
    private static EmailVerifyView? _instance;
    public static EmailVerifyView Instance
    {
        get
        {
            _instance ??= new EmailVerifyView();
            return _instance;
        }
    }

    public readonly EmailVerifyViewModel emailVerifyViewModel = new();

    public EmailVerifyView()
    {
        DataContext = emailVerifyViewModel;
        InitializeComponent();
    }
}