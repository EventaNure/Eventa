namespace Eventa.Domain
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int EventDateTimeId { get; set; }

        public EventDateTime EventDateTime { get; set; } = default!;

        public bool IsPurcharsed { get; set; }

        public DateTime ExpireAt { get; set; }

        public DateTime? CreationDateTime { get; set; }

        public ICollection<TicketInOrder> Tickets { get; set; } = [];

        public Guid QrToken { get; set; }

        public bool IsQrTokenUsed { get; set; }
    }
}
