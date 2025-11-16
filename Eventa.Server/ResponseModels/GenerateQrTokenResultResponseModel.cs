namespace Eventa.Server.ResponseModels
{
    public class GenerateQrTokenResultResponseModel
    {
        public string? CheckQrTokenUrl { get; set; }

        public DateTime? QrCodeUsingDateTime { get; set; }

        public bool IsQrTokenUsed { get; set; }
    }
}
