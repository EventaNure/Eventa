using System;

namespace Eventa.Models.Ordering;

public class GenerateQRCodeResponse
{
    public string CheckQrTokenUrl { get; set; } = string.Empty;

    public DateTime? QrCodeUsingDateTime { get; set; }

    public bool IsQrTokenUsed { get; set; }
}