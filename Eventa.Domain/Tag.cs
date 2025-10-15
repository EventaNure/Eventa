using System.ComponentModel.DataAnnotations;

namespace Eventa.Domain
{
    public class Tag
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public ICollection<EventTag> EventTags { get; set; } = [];
    }
}
