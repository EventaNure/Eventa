using System.ComponentModel.DataAnnotations;

namespace Eventa.Domain
{
    public class Event
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public TimeSpan Duration { get; set; }

        public ICollection<EventTag> EventTags { get; set; } = [];

        public ICollection<EventDateTime> EventDateTimes { get; set; } = [];

        public string ApplicationUserId { get; set; } = string.Empty;

        public Place Place { get; set; } = default!;

        public int PlaceId { get; set; }

        public EventStatus EventStatus {  get; set; }
    }
}
