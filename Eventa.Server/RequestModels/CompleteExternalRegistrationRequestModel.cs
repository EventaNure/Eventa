using System.ComponentModel.DataAnnotations;

namespace Eventa.Server.RequestModels
{
    public class CompleteExternalRegistrationRequestModel
    {
        [Required]
        [StringLength(32, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
        [StringLength(32, MinimumLength = 3)]
        public string? Organization { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
