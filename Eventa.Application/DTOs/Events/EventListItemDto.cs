namespace Eventa.Application.DTOs.Events
{
    public class EventListItemDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public double Price { get; set; }

        public DateTime FirstDateTime { get; set; }

        public DateTime LastDateTime { get; set; }

        public string Address { get; set; } = string.Empty;

        public byte[] ImageBytes { get; set; } = default!;
    }
}
