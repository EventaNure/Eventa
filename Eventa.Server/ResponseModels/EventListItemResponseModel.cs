namespace Eventa.Server.ResponseModels
{
    public class EventListItemResponseModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public double Price { get; set; }

        public DateTime FirstDateTime { get; set; }

        public DateTime LastDateTime { get; set; }

        public string Address { get; set; } = string.Empty;

        public int TicketsSold { get; set; }

        public string? ImageUrl { get; set; } = string.Empty;
    }
}
