using Avalonia.Controls;
using Eventa.ViewModels.Authentication;

namespace Eventa.Views.Authentication;

public partial class EmailVerifySuccessView : UserControl
{
    private static EmailVerifySuccessView? _instance;
    public static EmailVerifySuccessView Instance
    {
        get
        {
            _instance ??= new EmailVerifySuccessView();
            return _instance;
        }
    }

    public readonly EmailVerifySuccessViewModel emailVerifySuccessViewModel = new();

    public EmailVerifySuccessView()
    {
        DataContext = emailVerifySuccessViewModel;
        InitializeComponent();
    }
}