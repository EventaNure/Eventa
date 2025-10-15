using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Eventa.Models.Events;
using Eventa.ViewModels.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    private async void OnTagCheckChanged(object? sender, RoutedEventArgs e)
    {
        // await browseEventsViewModel.ApplyFiltersCommand.ExecuteAsync(null);
    }

    public async Task SelectTagByNameInUI(string tagName)
    {
        return Task.CompletedTask;
        // TODO: Fix browse tags from header not checking in browse view
        /*
        if (string.IsNullOrWhiteSpace(tagName))
            return;

        // Ensure tags are loaded in view model
        if (!browseEventsViewModel.Tags.Any())
        {
            await browseEventsViewModel.LoadTagsCommand.ExecuteAsync(null);
        }

        // Wait for UI to render
        await Task.Delay(100);

        // Find the ItemsControl first
        var itemsControl = this.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(ic => ic.ItemsSource == browseEventsViewModel.Tags);

        if (itemsControl == null)
            return;

        // Get all visual children from ItemsControl (the actual rendered items)
        var checkBoxes = new List<CheckBox>();

        foreach (var item in itemsControl.GetVisualChildren())
        {
            // Search within each container for checkboxes
            var containerCheckBoxes = item.GetVisualDescendants()
                .OfType<CheckBox>()
                .Where(cb => cb.Tag?.ToString() == "Tag");

            checkBoxes.AddRange(containerCheckBoxes);
        }

        if (!checkBoxes.Any())
        {
            // Fallback: search all descendants
            checkBoxes = itemsControl.GetVisualDescendants()
                .OfType<CheckBox>()
                .Where(cb => cb.DataContext is TagResponseModel)
                .ToList();
        }

        foreach (var checkBox in checkBoxes)
        {
            if (checkBox.DataContext is TagResponseModel tag)
            {
                if (tag.Name.Equals(tagName, System.StringComparison.OrdinalIgnoreCase))
                {
                    // Check this checkbox
                    checkBox.IsChecked = true;
                }
                else
                {
                    // Uncheck all other checkboxes
                    checkBox.IsChecked = false;
                }
            }
        }

        // Apply filters after updating checkboxes
        // await browseEventsViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        */
    }

}