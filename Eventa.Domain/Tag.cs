namespace Eventa.Domain
{
    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public IEnumerable<EventTag> EventTags { get; set; } = [];
    }
}
