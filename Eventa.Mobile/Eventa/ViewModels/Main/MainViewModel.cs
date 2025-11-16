using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Eventa.Controls;

namespace Eventa.ViewModels.Main;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private UserControl? _currentPage;
    [ObservableProperty]
    private UserControl? _currentDialog;

    public void ShowDialog()
    {
        CurrentDialog = DialogControl.Instance;
    }

    public void ShowImageDialog()
    {
        CurrentDialog = ZoomImageDialog.Instance;
    }

    public void ShowQRCodeDialog()
    {
        CurrentDialog = QRCodeDialog.Instance;
    }
}