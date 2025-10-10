using System.ComponentModel.DataAnnotations;

namespace Eventa.Server.RequestModels
{
    public class RegisterUserRequestModel
    {
        [Required]
        [StringLength(254, MinimumLength = 5)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z]).*$", ErrorMessage = "Password must have at least one lowercase and one upperrcase letter")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(32, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }
}
