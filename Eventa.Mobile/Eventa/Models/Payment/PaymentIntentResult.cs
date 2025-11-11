namespace Eventa.Models.Payment;

public class PaymentIntentResult
{
    public bool Success { get; set; }
    public string? ClientSecret { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Message { get; set; }
}