namespace Eventa.Application.DTOs.Events
{
    public class CreateEventDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }

        public int OrganizerId { get; set; }

        public IEnumerable<int> TagIds { get; set; } = [];

        public IEnumerable<DateTime> DateTimes { get; set; } = [];
    }
}
