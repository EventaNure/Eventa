using System.Collections.Generic;

namespace Eventa.Models.Payment;

public class PaymentRequest
{
    public string? CardNumber { get; set; }
    public long ExpiryMonth { get; set; }
    public long ExpiryYear { get; set; }
    public string? Cvc { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "uah";
    public string? Description { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}