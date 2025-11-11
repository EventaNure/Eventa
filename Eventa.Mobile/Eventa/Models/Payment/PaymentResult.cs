namespace Eventa.Models.Payment;

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
    public bool RequiresAction { get; set; }
    public string? ClientSecret { get; set; }
}