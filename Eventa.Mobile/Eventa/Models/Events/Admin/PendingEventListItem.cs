using System;

namespace Eventa.Models.Events.Admin;

public class PendingEventListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public double AverageRating { get; set; }
    public int RoundedAverageRating => (int)Math.Round(AverageRating);
}