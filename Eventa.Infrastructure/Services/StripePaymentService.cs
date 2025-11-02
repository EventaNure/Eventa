using Eventa.Application.DTOs.TicketInCarts;
using Eventa.Application.Services;
using Eventa.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Eventa.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly PaymentOptions _options;

        public StripePaymentService(IOptions<PaymentOptions> options) {
            _options = options.Value;
        }
        public async Task<string> CreateCheckoutSessionAsync(int orderId, IEnumerable<ItemWithPriceDto> items, string successUrl, string cancleUrl, string metadataName)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = items.Select(t => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = t.Price,
                        Currency = "uah",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = t.Name
                        }
                    },
                    Quantity = 1
                }).ToList(),
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancleUrl,
                Metadata = new Dictionary<string, string>
                {
                     { metadataName, orderId.ToString() }
                }
            };
            var service = new SessionService();
            var seccion = await service.CreateAsync(options);

            return seccion.Id;
        }

        public async Task<string> CreatePaymentIntentAsync(int orderId, string metadataName, double totalPrice)
        {
            var options = new PaymentIntentCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Amount = (int)Math.Ceiling(totalPrice) * 100,
                Currency = "uah",
                Metadata = new Dictionary<string, string>
                {
                     { metadataName, orderId.ToString() }
                }
            };
            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.Id;
        }

        public bool IsCheckoutSessionSuccess(string payload, string signature)
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret, throwOnApiVersionMismatch: false);

            return stripeEvent.Type == "checkout.session.completed";
        }

        public bool IsPaymentIntentSuccess(string payload, string signature)
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret, throwOnApiVersionMismatch: false);

            return stripeEvent.Type == "payment_intent.succeeded";
        }

        public string? GetMetadataFromPayment(string payload, string signature, string metadataName)
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret, throwOnApiVersionMismatch: false);

            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent == null)
            {
                return null;
            }

            var metadata = paymentIntent.Metadata;

            var metadataExists = metadata.TryGetValue(metadataName, out var metadataValue);

            return metadataValue;
        }

        public string? GetMetadataFromSession(string payload, string signature, string metadataName)
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret, throwOnApiVersionMismatch: false);

            var paymentIntent = stripeEvent.Data.Object as Session;

            if (paymentIntent == null)
            {
                return null;
            }

            var metadata = paymentIntent.Metadata;

            var metadataExists = metadata.TryGetValue(metadataName, out var metadataValue);

            return metadataValue;
        }
    }
}
