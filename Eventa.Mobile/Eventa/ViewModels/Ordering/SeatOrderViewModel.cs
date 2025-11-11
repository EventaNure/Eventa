using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Seats;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Eventa.ViewModels.Ordering;

public partial class SeatOrderViewModel : ObservableObject
{
    private static readonly List<Color> RowTypeColors = new()
    {
        Color.Parse("#FFB5B5"), // Pink
        Color.Parse("#FFD9A0"), // Orange
        Color.Parse("#B5C9FF"), // Blue
        Color.Parse("#B5FFB5"), // Green
        Color.Parse("#E0B5FF"), // Purple
        Color.Parse("#FFE5B5"), // Yellow
    };

    [ObservableProperty]
    private string _hallPlanUrl = string.Empty;

    [ObservableProperty]
    private string _eventName = string.Empty;

    [ObservableProperty]
    private string _eventDateTime = string.Empty;

    [ObservableProperty]
    private double _totalPrice = 0.0;

    [ObservableProperty]
    private ObservableCollection<RowTypeViewModel> _rowTypes = [];

    [ObservableProperty]
    private ObservableCollection<SeatViewModel> _selectedSeats = [];

    public void InsertFormData(FreeSeatsWithHallPlanResponseModel data, string eventName, string eventDescription,
        System.DateTime dateTime, string placeAddress)
    {
        HallPlanUrl = data.HallPlanUrl;
        EventName = eventName;
        EventDateTime = dateTime.ToString("dd MMM yyyy, HH:mm");

        RowTypes.Clear();
        SelectedSeats.Clear();

        int colorIndex = 0;

        foreach (var rowType in data.RowTypes)
        {
            var color = RowTypeColors[colorIndex % RowTypeColors.Count];
            colorIndex++;

            var rowTypeVm = new RowTypeViewModel
            {
                Name = rowType.Name,
                Color = new SolidColorBrush(color),
                Rows = new ObservableCollection<RowViewModel>()
            };

            // Calculate price range for this row type
            var allSeats = rowType.Rows.SelectMany(r => r.Seats).ToList();
            if (allSeats.Any())
            {
                var minPrice = allSeats.Min(s => s.Price);
                var maxPrice = allSeats.Max(s => s.Price);
                rowTypeVm.PriceRange = minPrice == maxPrice
                    ? $"[{minPrice:F2}]"
                    : $"[{minPrice:F2}-{maxPrice:F2}]";
            }

            foreach (var row in rowType.Rows)
            {
                var rowVm = new RowViewModel
                {
                    Id = row.Id,
                    RowNumber = row.RowNumber,
                    Seats = new ObservableCollection<SeatViewModel>()
                };

                foreach (var seat in row.Seats)
                {
                    var seatVm = new SeatViewModel(color)
                    {
                        Id = seat.Id,
                        SeatNumber = seat.SeatNumber,
                        Price = seat.Price,
                        RowTypeName = rowType.Name,
                        RowNumber = row.RowNumber,
                        SelectSeatCommand = new RelayCommand<SeatViewModel>(ToggleSeatSelection)
                    };
                    rowVm.Seats.Add(seatVm);
                }

                rowTypeVm.Rows.Add(rowVm);
            }

            RowTypes.Add(rowTypeVm);
        }
    }

    private void ToggleSeatSelection(SeatViewModel? seat)
    {
        if (seat == null) return;

        if (seat.IsSelected)
        {
            seat.IsSelected = false;
            SelectedSeats.Remove(seat);
        }
        else
        {
            seat.IsSelected = true;
            SelectedSeats.Add(seat);
        }

        CalculateTotalPrice();
    }

    private void CalculateTotalPrice()
    {
        TotalPrice = SelectedSeats.Sum(s => s.Price);
    }

    [RelayCommand]
    private void Buy()
    {
        if (SelectedSeats.Count == 0)
            return;

        // Implement purchase logic here
        // This would typically call an API service to complete the purchase
    }

    [RelayCommand]
    private void RemoveSeat(SeatViewModel? seat)
    {
        if (seat is null)
            return;

        seat.IsSelected = false;
        SelectedSeats.Remove(seat);
        CalculateTotalPrice();
    }
}