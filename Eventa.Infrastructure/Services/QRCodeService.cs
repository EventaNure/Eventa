using QRCoder;

namespace Eventa.Infrastructure.Services
{
    public class QRCodeService : IQRCodeService
    {
        public string GenerateQrCode(Guid value)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(value.ToString(), QRCodeGenerator.ECCLevel.Q);
            var qrCode = new SvgQRCode(qrData);
            return qrCode.GetGraphic(10);
        }
    }
}
