using System;

namespace Eventa.Models.Ordering;

public class GenerateQRCodeResponse
{
    public Guid? QrToken { get; set; }

    public DateTime? QrCodeUsingDateTime { get; set; }

    public bool IsQrTokenUsed { get; set; }
}