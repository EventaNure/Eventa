namespace Eventa.Domain
{
    public class Event
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }

        public IEnumerable<EventTag> EventTags { get; set; } = [];

        public IEnumerable<EventDateTime> EventDateTimes { get; set; } = [];

        public int OrganizerId { get; set; }
    }
}
