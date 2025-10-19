using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace Eventa.Controls;

public partial class DialogControl : UserControl
{
    private static DialogControl? _instance;
    private TaskCompletionSource<bool>? _taskCompletionSource;

    public static DialogControl Instance
    {
        get
        {
            _instance ??= new DialogControl();
            return _instance;
        }
    }

    public DialogControl()
    {
        InitializeComponent();
    }

    public Task<bool> Show(string title, string description, string noButton, string okButton)
    {
        // Create a new TaskCompletionSource for this dialog instance
        _taskCompletionSource = new TaskCompletionSource<bool>();

        // Set the text content
        TitleTextBlock.Text = title;
        DescriptionTextBlock.Text = description;
        NoButton.Content = noButton;
        OkButton.Content = okButton;

        // Show the dialog
        DialogOverlay.IsVisible = true;

        // Return the task that will be completed when user clicks a button
        return _taskCompletionSource.Task;
    }

    private void OnNoClick(object? sender, RoutedEventArgs e)
    {
        // Hide the dialog
        DialogOverlay.IsVisible = false;

        // Complete the task with false result
        _taskCompletionSource?.SetResult(false);
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        // Hide the dialog
        DialogOverlay.IsVisible = false;

        // Complete the task with true result
        _taskCompletionSource?.SetResult(true);
    }
}