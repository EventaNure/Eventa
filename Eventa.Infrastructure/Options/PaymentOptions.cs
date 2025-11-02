namespace Eventa.Infrastructure.Options
{
    public class PaymentOptions
    {
        public string Signature { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string PublicKey { get; set; } = string.Empty;

        public string WebhookSecret { get; set; } = string.Empty;
    }
}
