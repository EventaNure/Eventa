using Eventa.Application.DTOs.Tags;

namespace Eventa.Server.ResponseModels
{
    public class EventResponseModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public TimeSpan Duration { get; set; }

        public string OrganizerName { get; set; } = string.Empty;

        public string PlaceName { get; set; } = string.Empty;

        public string PlaceAddress { get; set; } = string.Empty;

        public IEnumerable<TagDto> Tags { get; set; } = [];

        public IEnumerable<DateTime> DateTimes { get; set; } = [];

        public string? ImageUrl { get; set; }
    }
}
