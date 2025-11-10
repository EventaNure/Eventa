using System.Text.Json.Serialization;

namespace Eventa.Models.Booking;

public class BookTicketRequestModel
{
    [JsonPropertyName("eventDateTimeId")]
    public int EventDateTimeId { get; set; }
    [JsonPropertyName("seatId")]
    public int SeatId { get; set; }
}