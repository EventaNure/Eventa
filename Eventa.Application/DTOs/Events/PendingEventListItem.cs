namespace Eventa.Application.DTOs.Events
{
    public class PendingEventListItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string OrganizerName { get; set; } = string.Empty;

        public string? OrganizationName { get; set; }

        public string? ImageUrl { get; set; }
    }
}
