using CommunityToolkit.Mvvm.ComponentModel;

namespace Eventa.Models.Events;

public partial class BrowseTagItemModel : ObservableObject
{
    [ObservableProperty]
    private int _tagId = -1;
    [ObservableProperty]
    private string _tagName = string.Empty;
    [ObservableProperty]
    private bool _isLastItem;
    [ObservableProperty]
    private bool _isError;
}