namespace Eventa.Server.RequestModels
{
    public class EventRequestModel
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public TimeSpan Duration { get; set; }

        public string OrganizerId { get; set; } = string.Empty;

        public int PlaceId { get; set; }

        public IEnumerable<int> TagIds { get; set; } = [];

        public IEnumerable<DateTime> DateTimes { get; set; } = [];

        public IFormFile ImageFile { get; set; } = default!;
    }
}
