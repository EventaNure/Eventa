
namespace Eventa.Infrastructure.Services
{
    public interface IQRCodeService
    {
        string GenerateQrCode(Guid value);
    }
}