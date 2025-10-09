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

    private void TextBoxRequired_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        TextBlock? requiredIndicator = textBox.Name switch
        {
            "UserNameTextBox" => UserNameRequired,
            "EmailTextBox" => EmailRequired,
            "PasswordTextBox" => PasswordRequired,
            "ConfirmPasswordTextBox" => ConfirmPasswordRequired,
            _ => null
        };
        requiredIndicator?.Text = string.IsNullOrWhiteSpace(textBox.Text) ? "*" : string.Empty;
    }
}