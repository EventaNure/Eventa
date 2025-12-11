namespace Eventa.Server.ResponseModels
{
    public class CompleteExternalRegistrationResultResponseModel
    {
        public string JwtToken { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
