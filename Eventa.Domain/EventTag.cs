namespace Eventa.Domain
{
    public class EventTag
    {
        public int EventId { get; set; }

        public Event Event { get; set; } = default!;

        public int TagId { get; set; }

        public Tag Tag { get; set; } = default!;
    }
}
