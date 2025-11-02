using Eventa.Application.DTOs.TicketInCarts;

namespace Eventa.Application.Services
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(int orderId, IEnumerable<ItemWithPriceDto> items, string successUrl, string cancleUrl, string metadataName);
        Task<string> CreatePaymentIntentAsync(int orderId, string metadataName, double totalPrice);
        string? GetMetadataFromPayment(string payload, string signature, string metadataName);
        string? GetMetadataFromSession(string payload, string signature, string metadataName);
        bool IsCheckoutSessionSuccess(string payload, string signature);
        bool IsPaymentIntentSuccess(string payload, string signature);
    }
}