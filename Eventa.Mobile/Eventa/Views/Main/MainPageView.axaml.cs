using Avalonia.Controls;
using Eventa.ViewModels;
using Eventa.ViewModels.Main;

namespace Eventa.Views.Main;

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