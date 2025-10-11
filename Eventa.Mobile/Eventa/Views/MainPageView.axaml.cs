using Avalonia.Controls;
using Eventa.ViewModels;

namespace Eventa.Views;

public partial class MainPageView : UserControl
{
    private static MainPageView? _instance;
    public static MainPageView Instance
    {
        get
        {
            _instance ??= new MainPageView();
            return _instance;
        }
    }

    public readonly MainPageViewModel mainPageViewModel = new();

    public MainPageView()
    {
        DataContext = mainPageViewModel;
        InitializeComponent();
    }
}