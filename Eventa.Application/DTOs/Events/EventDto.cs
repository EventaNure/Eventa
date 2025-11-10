using Eventa.Application.DTOs.Tags;

namespace Eventa.Application.DTOs.Events
{
    public class EventDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double MinPrice { get; set; }

        public double MaxPrice { get; set; }

        public TimeSpan Duration { get; set; }

        public string OrganizerName { get; set; } = string.Empty;

        public string PlaceName { get; set; } = string.Empty;

        public string PlaceAddress { get; set; } = string.Empty;

        public IEnumerable<TagDto> Tags { get; set; } = [];

        public IEnumerable<EventDateTimeDto> DateTimes { get; set; } = [];

        public string? ImageUrl { get; set; }
    }
}
