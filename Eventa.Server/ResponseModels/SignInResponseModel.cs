namespace Eventa.Server.ResponseModels
{
    public class SignInResponseModel
    {
        public string? UserId { get; set; }

        public string? JwtToken { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}
