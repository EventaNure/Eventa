using Avalonia.Controls;
using Eventa.ViewModels.Events;
using Mapsui.Extensions;
using Mapsui.UI.Avalonia;
using Mapsui.Widgets.InfoWidgets;

namespace Eventa.Views.Events;

public partial class SelectPlacesEventView : UserControl
{
    private static SelectPlacesEventView? _instance;
    public static SelectPlacesEventView Instance
    {
        get
        {
            _instance ??= new SelectPlacesEventView();
            return _instance;
        }
    }

    public readonly SelectPlacesEventViewModel selectPlacesEventViewModel = new();
    private MapControl? _mapControl;

    public SelectPlacesEventView()
    {
        DataContext = selectPlacesEventViewModel;
        InitializeComponent();
        InitializeMap();
    }

    private void InitializeMap()
    {
        _mapControl = new MapControl();
        LoggingWidget.ShowLoggingInMap = Mapsui.Widgets.ActiveMode.No;

        var mapTile = Mapsui.Tiling.OpenStreetMap.CreateTileLayer();
        mapTile.Attribution.Enabled = false;
        _mapControl.Map!.Layers.Add(mapTile);
        _mapControl.Map.Widgets.Clear();

        MapGrid.Children.Add(_mapControl);

        selectPlacesEventViewModel.SetMapControl(_mapControl);
    }
}