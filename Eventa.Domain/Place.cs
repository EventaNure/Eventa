using System.ComponentModel.DataAnnotations;

namespace Eventa.Domain
{
    public class Place
    {
        public int Id { get; set; }

        [MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;
    }
}
