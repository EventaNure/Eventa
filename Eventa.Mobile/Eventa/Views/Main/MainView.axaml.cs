using Avalonia.Controls;
using Eventa.ViewModels.Main;

namespace Eventa.Views.Main;

public partial class MainView : UserControl
{
    public static MainView Instance { get; private set; } = null!;

    public readonly MainViewModel mainViewModel = new();

    public MainView()
    {
        DataContext = mainViewModel;
        InitializeComponent();
        Instance = this;
        ChangePage(MainPageView.Instance);
    }

    public void ChangePage(UserControl newPage)
    {
        mainViewModel.CurrentPage = newPage;
    }
}