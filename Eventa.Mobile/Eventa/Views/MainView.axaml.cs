using Avalonia.Controls;
using Eventa.ViewModels;
using Eventa.Views.Authentication;

namespace Eventa.Views;

public partial class MainView : UserControl
{
    public static MainView Instance { get; private set; } = null!;

    public readonly MainViewModel mainViewModel = new();

    public MainView()
    {
        DataContext = mainViewModel;
        InitializeComponent();
        Instance = this;
        ChangePage(RegistrationView.Instance);
    }

    public void ChangePage(UserControl newPage)
    {
        mainViewModel.CurrentPage = newPage;
    }
}