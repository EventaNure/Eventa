using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Eventa.Models.Ordering;
using Eventa.Views.Main;
using SkiaSharp.QrCode.Image;
using System;
using System.IO;

namespace Eventa.Controls;

public partial class QRCodeDialog : UserControl
{
    private static QRCodeDialog? _instance;

    public static QRCodeDialog Instance
    {
        get
        {
            _instance ??= new QRCodeDialog();
            return _instance;
        }
    }

    public QRCodeDialog()
    {
        InitializeComponent();
    }

    public void Show(OrderListItemResponseModel order, GenerateQRCodeResponse qrCodeData)
    {
        MainView.Instance.mainViewModel.ShowQRCodeDialog();

        // Set event information
        EventNameTextBlock.Text = order.EventName;
        EventDateTextBlock.Text = order.EventDateTime.ToString("dddd, MMMM dd, yyyy - HH:mm");
        TotalCostTextBlock.Text = $"${order.TotalCost:F2}";

        // Clear and populate tickets
        TicketsStackPanel.Children.Clear();
        foreach (var ticket in order.Tickets)
        {
            var ticketBorder = CreateTicketBorder(ticket);
            TicketsStackPanel.Children.Add(ticketBorder);
        }

        // Generate and display QR code
        GenerateQRCode(qrCodeData.CheckQrTokenUrl);

        // Show status if already used
        if (qrCodeData.IsQrTokenUsed && qrCodeData.QrCodeUsingDateTime.HasValue)
        {
            StatusTextBlock.Text = $"⚠️ This QR code was already scanned on {qrCodeData.QrCodeUsingDateTime.Value:MM/dd/yyyy HH:mm}";
            StatusTextBlock.Foreground = Avalonia.Media.Brushes.OrangeRed;
            StatusTextBlock.IsVisible = true;
        }
        else
        {
            StatusTextBlock.IsVisible = false;
        }

        // Show the dialog
        DialogOverlay.IsVisible = true;
    }

    private static Border CreateTicketBorder(TicketResponseModel ticket)
    {
        var border = new Border
        {
            Background = Avalonia.Media.Brushes.White,
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0E0E0")),
            BorderThickness = new Avalonia.Thickness(1),
            CornerRadius = new Avalonia.CornerRadius(6),
            Padding = new Avalonia.Thickness(12, 8)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
        };

        // Seat icon/info
        var seatStackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 6
        };

        var seatIcon = new TextBlock
        {
            Text = "🪑",
            FontSize = 16,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var seatInfo = new StackPanel
        {
            Spacing = 2
        };

        var rowTypeText = new TextBlock
        {
            Text = ticket.RowTypeName,
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#333333"))
        };

        var seatDetailsText = new TextBlock
        {
            Text = $"Row {ticket.Row}, Seat {ticket.SeatNumber}",
            FontSize = 11,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#666666"))
        };

        seatInfo.Children.Add(rowTypeText);
        seatInfo.Children.Add(seatDetailsText);
        seatStackPanel.Children.Add(seatIcon);
        seatStackPanel.Children.Add(seatInfo);

        Grid.SetColumn(seatStackPanel, 0);
        grid.Children.Add(seatStackPanel);

        // Price
        var priceText = new TextBlock
        {
            Text = $"${ticket.Price:F2}",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF6B35")),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        Grid.SetColumn(priceText, 2);
        grid.Children.Add(priceText);

        border.Child = grid;
        return border;
    }

    private void GenerateQRCode(string qrToken)
    {
        try
        {
            string? qrCode = qrToken;

            if (string.IsNullOrEmpty(qrCode))
            {
                StatusTextBlock.Text = $"QR Code was not generated!";
                StatusTextBlock.Foreground = Avalonia.Media.Brushes.OrangeRed;
                StatusTextBlock.IsVisible = true;
                return;
            }
            else
            {
                StatusTextBlock.IsVisible = false;
            }
            // Get the PNG bytes directly
            var qrImage = QRCodeImageBuilder.GetPngBytes(qrCode);

            // Create a memory stream from the PNG bytes
            var stream = new MemoryStream(qrImage)
            {
                Position = 0
            };

            // Create Avalonia bitmap directly from the PNG bytes
            var bitmap = new Bitmap(stream);
            QRCodeImage.Source = bitmap;
        }
        catch (Exception ex)
        {
            // Handle error - show error message
            StatusTextBlock.Text = $"Error generating QR code: {ex.Message}";
            StatusTextBlock.Foreground = Avalonia.Media.Brushes.Red;
            StatusTextBlock.IsVisible = true;
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        DialogOverlay.IsVisible = false;
        MainPageView.Instance.mainPageViewModel.NavigateToMyTicketsCommand.Execute(null);
    }
}