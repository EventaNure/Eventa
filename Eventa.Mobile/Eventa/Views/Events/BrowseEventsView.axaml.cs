using Avalonia.Controls;
using Eventa.ViewModels.Events;

namespace Eventa.Views.Events;

public partial class BrowseEventsView : UserControl
{
    private static BrowseEventsView? _instance;
    public static BrowseEventsView Instance
    {
        get
        {
            _instance ??= new BrowseEventsView();
            return _instance;
        }
    }

    public readonly BrowseEventsViewModel browseEventsViewModel = new();

    public BrowseEventsView()
    {
        DataContext = browseEventsViewModel;
        InitializeComponent();
    }
}