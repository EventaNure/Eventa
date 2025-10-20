using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Eventa.Controls;

namespace Eventa.ViewModels.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private UserControl? _currentPage;
    [ObservableProperty]
    private UserControl? _currentDialog;

    public MainViewModel()
    {
        _currentDialog = DialogControl.Instance;
    }
}