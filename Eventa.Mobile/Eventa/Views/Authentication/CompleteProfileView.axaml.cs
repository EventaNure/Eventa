using Avalonia.Controls;
using Eventa.ViewModels.Authentication;

namespace Eventa.Views.Authentication;

public partial class CompleteProfileView : UserControl
{
    private static CompleteProfileView? _instance;
    public static CompleteProfileView Instance
    {
        get
        {
            _instance ??= new CompleteProfileView();
            return _instance;
        }
    }

    public readonly CompleteProfileViewModel completeProfileViewModel = new();

    public CompleteProfileView()
    {
        DataContext = completeProfileViewModel;
        InitializeComponent();
    }

    private void TextBoxRequired_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        TextBlock? requiredIndicator = textBox.Name switch
        {
            "NameTextBox" => NameRequired,
            _ => null
        };

        if (requiredIndicator != null)
        {
            requiredIndicator.Text = string.IsNullOrWhiteSpace(textBox.Text) ? "*" : string.Empty;
        }
    }
}