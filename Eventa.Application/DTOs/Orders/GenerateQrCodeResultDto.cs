namespace Eventa.Application.DTOs.Orders
{
    public class GenerateQrCodeResultDto
    {
        public string? QrCode { get; set; }

        public DateTime? QrCodeUsingDateTime { get; set; }

        public bool IsQrTokenUsed { get; set; }
    }
}
