namespace Eventa.Server.ResponseModels
{
    public class GoogleLoginResponseModel
    {
        public string UserId { get; set; } = string.Empty;

        public string JwtToken { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsLogin { get; set; }
    }
}
