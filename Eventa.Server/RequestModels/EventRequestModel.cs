using System.ComponentModel.DataAnnotations;

namespace Eventa.Server.RequestModels
{
    public class EventRequestModel
    {
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [StringLength(3000, MinimumLength = 300)]
        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public TimeSpan Duration { get; set; }

        public string OrganizerId { get; set; } = string.Empty;

        public int PlaceId { get; set; }

        public List<int> TagIds { get; set; } = [];

        public List<DateTime> DateTimes { get; set; } = [];

        public IFormFile ImageFile { get; set; } = default!;
    }
}
