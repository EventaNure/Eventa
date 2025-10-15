using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eventa.Controls;

public partial class ImageCarousel : UserControl
{
    private List<Border> _dotIndicators = [];
    private DispatcherTimer? _autoPlayTimer;

    public static readonly StyledProperty<IEnumerable<string>> ImagesProperty =
        AvaloniaProperty.Register<ImageCarousel, IEnumerable<string>>(nameof(Images), defaultValue: []);

    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<ImageCarousel, int>(nameof(CurrentIndex), defaultValue: 0);

    public static readonly StyledProperty<bool> AutoPlayProperty =
        AvaloniaProperty.Register<ImageCarousel, bool>(nameof(AutoPlay), defaultValue: true);

    public static readonly StyledProperty<TimeSpan> AutoPlayIntervalProperty =
        AvaloniaProperty.Register<ImageCarousel, TimeSpan>(nameof(AutoPlayInterval), defaultValue: TimeSpan.FromSeconds(3));

    public IEnumerable<string> Images
    {
        get => GetValue(ImagesProperty);
        set => SetValue(ImagesProperty, value);
    }

    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    public bool AutoPlay
    {
        get => GetValue(AutoPlayProperty);
        set => SetValue(AutoPlayProperty, value);
    }

    public TimeSpan AutoPlayInterval
    {
        get => GetValue(AutoPlayIntervalProperty);
        set => SetValue(AutoPlayIntervalProperty, value);
    }

    public ImageCarousel()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
        this.DetachedFromVisualTree += OnDetachedFromVisualTree;

        InitializeAutoPlay();
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateCarousel();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        StopAutoPlay();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ImagesProperty)
        {
            UpdateCarousel();
        }
        else if (change.Property == CurrentIndexProperty)
        {
            UpdateDotIndicators();
            UpdateButtonStates();
            ResetAutoPlayTimer();
        }
        else if (change.Property == AutoPlayProperty)
        {
            if (AutoPlay)
                StartAutoPlay();
            else
                StopAutoPlay();
        }
        else if (change.Property == AutoPlayIntervalProperty)
        {
            if (_autoPlayTimer != null)
            {
                _autoPlayTimer.Interval = AutoPlayInterval;
            }
        }
    }

    private void UpdateCarousel()
    {
        var images = Images?.ToList() ?? [];

        MainCarousel.ItemsSource = images;

        if (images.Count > 0)
        {
            InitializeDots(images.Count);
            MainCarousel.SelectedIndex = 0;
        }
        else
        {
            DotsPanel.Children.Clear();
            _dotIndicators.Clear();
        }
    }

    private void InitializeDots(int count)
    {
        DotsPanel.Children.Clear();
        _dotIndicators.Clear();

        for (int i = 0; i < count; i++)
        {
            var index = i;
            var dot = new Border
            {
                Classes = { "dot" }
            };

            var button = new Button
            {
                Content = dot,
                Classes = { "dot-button" }
            };

            button.Click += (s, e) =>
            {
                GoToImage(index);
                ResetAutoPlayTimer();
            };

            DotsPanel.Children.Add(button);
            _dotIndicators.Add(dot);
        }

        UpdateDotIndicators();
    }

    private void UpdateDotIndicators()
    {
        for (int i = 0; i < _dotIndicators.Count; i++)
        {
            if (i == CurrentIndex)
            {
                _dotIndicators[i].Classes.Add("active");
            }
            else
            {
                _dotIndicators[i].Classes.Remove("active");
            }
        }
    }

    private void UpdateButtonStates()
    {
        var images = Images?.ToList() ?? [];
        var imageCount = images.Count;
        PrevButton.IsEnabled = imageCount > 1;
        NextButton.IsEnabled = imageCount > 1;
    }

    private void OnPrevClick(object? sender, RoutedEventArgs e)
    {
        Previous();
        ResetAutoPlayTimer();
    }

    private void OnNextClick(object? sender, RoutedEventArgs e)
    {
        Next();
        ResetAutoPlayTimer();
    }

    public void GoToImage(int index)
    {
        var images = Images?.ToList() ?? [];
        if (images.Count == 0) return;

        // Wrap around if needed
        if (index < 0) index = images.Count - 1;
        if (index >= images.Count) index = 0;

        MainCarousel.SelectedIndex = index;
    }

    public void Next()
    {
        var images = Images?.ToList() ?? [];
        if (images.Count == 0) return;

        var nextIndex = CurrentIndex + 1;
        if (nextIndex >= images.Count) nextIndex = 0;

        GoToImage(nextIndex);
    }

    public void Previous()
    {
        var images = Images?.ToList() ?? [];
        if (images.Count == 0) return;

        var prevIndex = CurrentIndex - 1;
        if (prevIndex < 0) prevIndex = images.Count - 1;

        GoToImage(prevIndex);
    }

    private void InitializeAutoPlay()
    {
        _autoPlayTimer = new DispatcherTimer
        {
            Interval = AutoPlayInterval
        };
        _autoPlayTimer.Tick += OnAutoPlayTick;

        if (AutoPlay)
        {
            StartAutoPlay();
        }
    }

    private void OnAutoPlayTick(object? sender, EventArgs e)
    {
        Next();
    }

    private void StartAutoPlay()
    {
        _autoPlayTimer?.Start();
    }

    private void StopAutoPlay()
    {
        _autoPlayTimer?.Stop();
    }

    private void ResetAutoPlayTimer()
    {
        if (_autoPlayTimer != null && AutoPlay)
        {
            _autoPlayTimer.Stop();
            _autoPlayTimer.Start();
        }
    }

    public void PauseAutoPlay()
    {
        StopAutoPlay();
    }

    public void ResumeAutoPlay()
    {
        if (AutoPlay)
        {
            StartAutoPlay();
        }
    }
}