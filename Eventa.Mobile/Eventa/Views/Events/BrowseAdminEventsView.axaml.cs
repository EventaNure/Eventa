using Avalonia.Controls;
using Eventa.ViewModels.Events;

namespace Eventa.Views.Events;

public partial class BrowseAdminEventsView : UserControl
{
    private static BrowseAdminEventsView? _instance;
    public static BrowseAdminEventsView Instance
    {
        get
        {
            _instance ??= new BrowseAdminEventsView();
            return _instance;
        }
    }

    public readonly BrowseAdminEventsViewModel browseAdminEventsViewModel = new();

    public BrowseAdminEventsView()
    {
        DataContext = browseAdminEventsViewModel;
        InitializeComponent();
    }
}