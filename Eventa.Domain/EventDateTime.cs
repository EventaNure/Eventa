namespace Eventa.Domain
{
    public class EventDateTime
    {
        public int Id { get; set; }

        public Event Event { get; set; } = new Event();

        public DateTime StartDateTime { get; set; }
    }
}
