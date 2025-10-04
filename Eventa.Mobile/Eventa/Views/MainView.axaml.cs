using Avalonia.Controls;
using Eventa.ViewModels;

namespace Eventa.Views;

public partial class MainView : UserControl
{
    private readonly MainViewModel mainViewModel = new();

    public MainView()
    {
        DataContext = mainViewModel;
        InitializeComponent();
    }
}