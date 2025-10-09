using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Eventa.Infrastructure
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(32, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(6, MinimumLength = 6)]
        public string? VerificationCode { get; set; } = string.Empty;
    }
}
