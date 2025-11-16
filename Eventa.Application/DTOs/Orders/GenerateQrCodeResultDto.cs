namespace Eventa.Application.DTOs.Orders
{
    public class GenerateQrCodeResultDto
    {
        public Guid? QrToken { get; set; }

        public DateTime? QrCodeUsingDateTime { get; set; }

        public bool IsQrTokenUsed { get; set; }
    }
}