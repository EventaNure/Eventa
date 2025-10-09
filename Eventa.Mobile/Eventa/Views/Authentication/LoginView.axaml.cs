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


    private void TextBoxRequired_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        TextBlock? requiredIndicator = textBox.Name switch
        {
            "EmailTextBox" => EmailRequired,
            "PasswordTextBox" => PasswordRequired,
            _ => null
        };
        requiredIndicator?.Text = string.IsNullOrWhiteSpace(textBox.Text) ? "*" : string.Empty;
    }
}