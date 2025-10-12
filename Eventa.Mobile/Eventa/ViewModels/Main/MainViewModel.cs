using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Eventa.ViewModels.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private UserControl? _currentPage;
}