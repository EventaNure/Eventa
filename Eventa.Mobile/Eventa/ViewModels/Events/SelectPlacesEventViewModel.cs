using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events.Organizer;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Avalonia;
using System.Collections.Generic;
using System.Linq;

namespace Eventa.ViewModels.Events;

public partial class SelectPlacesEventViewModel : ObservableObject
{
    private MapControl? _mapControl;
    private WritableLayer? _placesLayer;
    private List<PlaceResponseModel> _places = [];
    private PlaceResponseModel? _selectedPlace;

    [ObservableProperty]
    private string _selectedPlaceName = "[Place name]";

    [ObservableProperty]
    private string _selectedPlaceAddress = "[Place address]";

    [ObservableProperty]
    private bool _hasSelectedPlace;

    public void SetMapControl(MapControl mapControl)
    {
        _mapControl = mapControl;

        if (_mapControl?.Map != null)
        {
            _mapControl.Map.Info += OnMapInfo;
        }
    }

    public void LoadPlaces(List<PlaceResponseModel> places)
    {
        ClearSelection();

        _places = places;

        if (_mapControl?.Map == null || places.Count == 0)
            return;

        if (_placesLayer != null && _mapControl.Map.Layers.Contains(_placesLayer))
        {
            _mapControl.Map.Layers.Remove(_placesLayer);
        }

        _placesLayer = new WritableLayer
        {
            Name = "Places",
            Style = null
        };

        foreach (var place in places)
        {
            var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(
                place.Longitude,
                place.Latitude
            );

            var pointFeature = new PointFeature(sphericalMercatorCoordinate.ToMPoint());

            pointFeature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.7,
                Fill = new Brush(Color.FromString("#E07A5F")),
                Outline = new Pen(Color.White, 2)
            });
            
            pointFeature["PlaceId"] = place.Id;
            pointFeature["PlaceName"] = place.Name;
            pointFeature["PlaceAddress"] = place.Address;

            _placesLayer.Add(pointFeature);
        }

        _mapControl.Map.Layers.Add(_placesLayer);

        if (_placesLayer.Extent != null)
        {
            var extent = _placesLayer.Extent;
            var expandedExtent = extent.Grow(extent.Width * 0.5, extent.Height * 0.5);
            _mapControl.Map.Navigator.ZoomToBox(expandedExtent);
        }
    }

    private void OnMapInfo(object? sender, MapInfoEventArgs e)
    {
        var mapInfo = e.GetMapInfo(_mapControl?.Map?.Layers!);

        if (mapInfo?.Feature == null)
            return;

        var feature = mapInfo.Feature;

        if (feature["PlaceId"] is int placeId)
        {
            _selectedPlace = _places.FirstOrDefault(p => p.Id == placeId);

            if (_selectedPlace != null)
            {
                SelectedPlaceName = _selectedPlace.Name;
                SelectedPlaceAddress = _selectedPlace.Address;
                HasSelectedPlace = true;

                HighlightSelectedPlace(feature);
            }
        }
    }

    private void HighlightSelectedPlace(IFeature selectedFeature)
    {
        if (_placesLayer == null)
            return;

        foreach (var feature in _placesLayer.GetFeatures())
        {
            feature.Styles.Clear();
            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.7,
                Fill = new Brush(Color.FromString("#E07A5F")),
                Outline = new Pen(Color.White, 2)
            });
        }

        // Highlight the selected feature
        selectedFeature.Styles.Clear();
        selectedFeature.Styles.Add(new SymbolStyle
        {
            SymbolScale = 1.0,
            Fill = new Brush(Color.FromString("#FFB5B5")),
            Outline = new Pen(Color.FromString("#E07A5F"), 3)
        });

        _mapControl?.Map?.RefreshData();
    }

    [RelayCommand]
    private void SelectPlace()
    {
        if (_selectedPlace == null)
            return;

        OnPlaceSelected?.Invoke(_selectedPlace);
    }

    public System.Action<PlaceResponseModel>? OnPlaceSelected { get; set; }

    public void ClearSelection()
    {
        _selectedPlace = null;
        SelectedPlaceName = "[Place name]";
        SelectedPlaceAddress = "[Place address]";
        HasSelectedPlace = false;

        if (_placesLayer != null)
        {
            foreach (var feature in _placesLayer.GetFeatures())
            {
                feature.Styles.Clear();
                feature.Styles.Add(new SymbolStyle
                {
                    SymbolScale = 0.7,
                    Fill = new Brush(Color.FromString("#E07A5F")),
                    Outline = new Pen(Color.White, 2)
                });
            }
            _mapControl?.Map?.RefreshData();
        }
    }
}