using Eventa.Models.Payment;
using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eventa.Services
{
    public class StripePaymentService
    {
        private readonly string _secretKey;
        private readonly bool _isTestMode;

        public StripePaymentService(string publishableKey, string secretKey)
        {
            _secretKey = secretKey;
            _isTestMode = _secretKey.StartsWith("sk_test_");
            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<PaymentResult> ProcessCardPayment(PaymentRequest request, string sessionId)
        {
            try
            {
                // Retrieve the existing PaymentIntent from Stripe
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(sessionId);

                // Verify the PaymentIntent exists and is in a valid state
                if (paymentIntent == null)
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message = "Payment intent not found"
                    };
                }

                if (paymentIntent.Status == "succeeded" || paymentIntent.Status == "canceled")
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message = $"Payment intent is already {paymentIntent.Status}"
                    };
                }

                // Create payment method from card details
                var paymentMethodId = await CreateCardPaymentMethod(request);

                // Attach payment method and confirm the payment
                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                };

                var confirmedIntent = await service.ConfirmAsync(sessionId, options);

                return new PaymentResult
                {
                    Success = confirmedIntent.Status == "succeeded",
                    TransactionId = confirmedIntent.Id,
                    Message = confirmedIntent.Status == "succeeded" ? "Payment successful" : "Payment requires additional action",
                    RequiresAction = confirmedIntent.Status == "requires_action",
                    ClientSecret = confirmedIntent.ClientSecret
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResult
                {
                    Success = false,
                    Message = ex.StripeError?.Message ?? "Payment processing failed"
                };
            }
        }

        private async Task<string> CreateCardPaymentMethod(PaymentRequest request)
        {
            if (_isTestMode)
            {
                return await CreateTestPaymentMethod(request);
            }

            var options = new PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new PaymentMethodCardOptions
                {
                    Number = request.CardNumber!.Replace(" ", ""),
                    ExpMonth = request.ExpiryMonth,
                    ExpYear = request.ExpiryYear,
                    Cvc = request.Cvc
                }
            };

            var service = new PaymentMethodService();
            var paymentMethod = await service.CreateAsync(options);
            return paymentMethod.Id;
        }

        private async Task<string> CreateTestPaymentMethod(PaymentRequest request)
        {
            var cardNumber = request.CardNumber?.Replace(" ", "");
            string testToken = cardNumber switch
            {
                "4242424242424242" or "4000056655665556" => "pm_card_visa",
                "5555555555554444" => "pm_card_mastercard",
                "378282246310005" or "371449635398431" => "pm_card_amex",
                "4000002500003155" => "pm_card_visa_chargeDeclined",
                "4000000000009995" => "pm_card_chargeDeclined",
                _ => "pm_card_visa",
            };
            return testToken;
        }
    }
}