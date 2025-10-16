using Avalonia.Controls;
using Eventa.ViewModels.Events;

namespace Eventa.Views.Events;

public partial class BrowseOrganizerEventsView : UserControl
{
    private static BrowseOrganizerEventsView? _instance;
    public static BrowseOrganizerEventsView Instance
    {
        get
        {
            _instance ??= new BrowseOrganizerEventsView();
            return _instance;
        }
    }

    public readonly BrowseOrganizerEventsViewModel browseOrganizerEventsViewModel = new();

    public BrowseOrganizerEventsView()
    {
        DataContext = browseOrganizerEventsViewModel;
        InitializeComponent();
    }
}