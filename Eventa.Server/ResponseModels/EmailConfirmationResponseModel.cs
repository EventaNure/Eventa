namespace Eventa.Server.ResponseModels
{
    public class EmailConfirmationResponseModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
