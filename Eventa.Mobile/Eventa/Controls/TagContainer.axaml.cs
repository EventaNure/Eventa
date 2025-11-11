using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Eventa.Controls;

public partial class TagContainer : UserControl
{
    private WrapPanel? _selectedTagsPanel;
    private WrapPanel? _availableTagsPanel;
    private Border? _dropdownPanel;
    private Border? _mainBorder;

    public static readonly StyledProperty<ObservableCollection<SelectableTag>> SelectedTagsProperty =
        AvaloniaProperty.Register<TagContainer, ObservableCollection<SelectableTag>>(
            nameof(SelectedTags));

    public static readonly StyledProperty<ObservableCollection<SelectableTag>> AvailableTagsProperty =
        AvaloniaProperty.Register<TagContainer, ObservableCollection<SelectableTag>>(
            nameof(AvailableTags));

    public ObservableCollection<SelectableTag> SelectedTags
    {
        get => GetValue(SelectedTagsProperty);
        set => SetValue(SelectedTagsProperty, value);
    }

    public ObservableCollection<SelectableTag> AvailableTags
    {
        get => GetValue(AvailableTagsProperty);
        set => SetValue(AvailableTagsProperty, value);
    }

    public TagContainer()
    {
        InitializeComponent();

        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _selectedTagsPanel = this.FindControl<WrapPanel>("SelectedTagsPanel");
        _availableTagsPanel = this.FindControl<WrapPanel>("AvailableTagsPanel");
        _dropdownPanel = this.FindControl<Border>("DropdownPanel");
        _mainBorder = this.FindControl<Border>("MainBorder");

        if (_mainBorder != null)
        {
            _mainBorder.PointerPressed += ToggleDropdown;
        }

        // Subscribe to property changes
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == SelectedTagsProperty)
            {
                var collection = SelectedTags;
                if (collection != null)
                {
                    collection.CollectionChanged -= OnSelectedTagsChanged;
                    collection.CollectionChanged += OnSelectedTagsChanged;
                    RemoveDuplicatesFromAvailable();
                    RebuildSelectedTags();
                }
            }
            else if (e.Property == AvailableTagsProperty)
            {
                var collection = AvailableTags;
                if (collection != null)
                {
                    collection.CollectionChanged -= OnAvailableTagsChanged;
                    collection.CollectionChanged += OnAvailableTagsChanged;
                    RemoveDuplicatesFromAvailable();
                    RebuildAvailableTags();
                }
            }
        };

        // Initial build
        if (SelectedTags != null)
        {
            SelectedTags.CollectionChanged += OnSelectedTagsChanged;
        }

        if (AvailableTags != null)
        {
            AvailableTags.CollectionChanged += OnAvailableTagsChanged;
        }

        // Remove duplicates before initial build
        RemoveDuplicatesFromAvailable();

        RebuildSelectedTags();
        RebuildAvailableTags();

        SelectedTags = [];
        this.Loaded -= OnLoaded;
    }

    private void RemoveDuplicatesFromAvailable()
    {
        if (SelectedTags == null || AvailableTags == null || SelectedTags.Count == 0)
            return;

        // Get IDs of selected tags
        var selectedIds = SelectedTags.Select(t => t.Id).ToHashSet();

        // Remove duplicates from available tags
        var duplicates = AvailableTags.Where(t => selectedIds.Contains(t.Id)).ToList();

        foreach (var duplicate in duplicates)
        {
            AvailableTags.Remove(duplicate);
        }
    }

    private void OnSelectedTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RemoveDuplicatesFromAvailable();
        RebuildSelectedTags();
    }

    private void OnAvailableTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildAvailableTags();
    }

    private void RebuildSelectedTags()
    {
        if (_selectedTagsPanel == null || SelectedTags == null)
            return;

        _selectedTagsPanel.Children.Clear();

        foreach (var tag in SelectedTags)
        {
            var tagBorder = CreateSelectedTagElement(tag);
            _selectedTagsPanel.Children.Add(tagBorder);
        }
    }

    private void RebuildAvailableTags()
    {
        if (_availableTagsPanel == null || AvailableTags == null)
            return;

        _availableTagsPanel.Children.Clear();

        foreach (var tag in AvailableTags)
        {
            var tagBorder = CreateAvailableTagElement(tag);
            _availableTagsPanel.Children.Add(tagBorder);
        }
    }

    private Border CreateSelectedTagElement(SelectableTag tag)
    {
        var textBlock = new TextBlock
        {
            Text = tag.Name,
            FontFamily = new FontFamily("avares://Eventa/Assets/Fonts/Kadwa#Kadwa"),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12
        };

        var closePath = new Path
        {
            Width = 10,
            Height = 10,
            Data = Geometry.Parse("M 0,0 L 10,10 M 10,0 L 0,10"),
            Stroke = new SolidColorBrush(Color.Parse("#666666")),
            StrokeThickness = 2
        };

        var closeButton = new Button
        {
            Width = 14,
            Height = 14,
            Padding = new Thickness(0),
            Margin = new Thickness(4, 0, 0, 0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand),
            Content = closePath
        };

        closeButton.Click += (s, e) =>
        {
            SelectedTags?.Remove(tag);
            AvailableTags?.Add(tag);
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { textBlock, closeButton }
        };

        var border = new Border
        {
            Classes = { "selectedTag" },
            Child = stackPanel
        };

        return border;
    }

    private Border CreateAvailableTagElement(SelectableTag tag)
    {
        var textBlock = new TextBlock
        {
            Text = tag.Name,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("avares://Eventa/Assets/Fonts/Kadwa#Kadwa"),
            FontSize = 12
        };

        var plusText = new TextBlock
        {
            Text = "+",
            Margin = new Thickness(4, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("avares://Eventa/Assets/Fonts/Kadwa#Kadwa"),
            FontSize = 14,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#666666"))
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { textBlock, plusText }
        };

        var border = new Border
        {
            Classes = { "availableTag" },
            Child = stackPanel,
            Tag = tag
        };

        border.PointerPressed += (s, e) =>
        {
            AvailableTags?.Remove(tag);
            SelectedTags?.Add(tag);
            if (_dropdownPanel != null)
                _dropdownPanel.IsVisible = false;
        };

        return border;
    }

    private void ToggleDropdown(object? sender, PointerPressedEventArgs e)
    {
        if (_dropdownPanel != null)
        {
            _dropdownPanel.IsVisible = !_dropdownPanel.IsVisible;
        }
    }
}

public partial class SelectableTag : ObservableObject
{
    [ObservableProperty]
    private int _id;
    [ObservableProperty]
    private bool _isSelected;
    [ObservableProperty]
    private string _name = string.Empty;
}