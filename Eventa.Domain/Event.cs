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

        public TimeSpan Duration { get; set; }

        public ICollection<EventTag> EventTags { get; set; } = [];

        public ICollection<EventDateTime> EventDateTimes { get; set; } = [];

        public int OrganizerId { get; set; }
    }
}
