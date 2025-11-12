using Eventa.Models.Payment;
using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eventa.Services
{
    public class StripePaymentService
    {
        private readonly string _publishableKey;
        private readonly string _secretKey;
        private readonly bool _isTestMode;

        public StripePaymentService(string publishableKey, string secretKey)
        {
            _publishableKey = publishableKey;
            _secretKey = secretKey;
            _isTestMode = _secretKey.StartsWith("sk_test_");
            StripeConfiguration.ApiKey = _secretKey;
        }

        // Create PaymentIntent for both card and Google Pay
        public async Task<PaymentIntentResult> CreatePaymentIntent(PaymentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convert to cents
                    Currency = request.Currency ?? "uah",
                    Description = request.Description,
                    Metadata = request.Metadata,
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                return new PaymentIntentResult
                {
                    Success = true,
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id
                };
            }
            catch (StripeException ex)
            {
                return new PaymentIntentResult
                {
                    Success = false,
                    Message = ex.StripeError?.Message ?? "Failed to create payment intent"
                };
            }
        }

        public async Task<PaymentResult> ProcessCardPayment(PaymentRequest request)
        {
            try
            {
                var intentResult = await CreatePaymentIntent(request);
                if (!intentResult.Success)
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message = intentResult.Message
                    };
                }

                var paymentMethodId = await CreateCardPaymentMethod(request);

                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.ConfirmAsync(intentResult.PaymentIntentId, options);

                return new PaymentResult
                {
                    Success = paymentIntent.Status == "succeeded",
                    TransactionId = paymentIntent.Id,
                    Message = paymentIntent.Status == "succeeded" ? "Payment successful" : "Payment requires additional action",
                    RequiresAction = paymentIntent.Status == "requires_action",
                    ClientSecret = paymentIntent.ClientSecret
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

        public async Task<PaymentResult> ProcessGooglePayPayment(string paymentMethodId, PaymentRequest request)
        {
            try
            {
                // Create PaymentIntent
                var intentResult = await CreatePaymentIntent(request);
                if (!intentResult.Success)
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message = intentResult.Message
                    };
                }

                var options = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.ConfirmAsync(intentResult.PaymentIntentId, options);

                return new PaymentResult
                {
                    Success = paymentIntent.Status == "succeeded",
                    TransactionId = paymentIntent.Id,
                    Message = paymentIntent.Status == "succeeded" ? "Payment successful" : "Payment processing",
                    RequiresAction = paymentIntent.Status == "requires_action"
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResult
                {
                    Success = false,
                    Message = ex.StripeError?.Message ?? "Google Pay payment failed"
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

            // Use Stripe's test card tokens
            string testToken;

            switch (cardNumber)
            {
                case "4242424242424242":
                case "4000056655665556":
                    testToken = "pm_card_visa";
                    break;
                case "5555555555554444":
                    testToken = "pm_card_mastercard";
                    break;
                case "378282246310005":
                case "371449635398431":
                    testToken = "pm_card_amex";
                    break;
                case "4000002500003155": 
                    testToken = "pm_card_visa_chargeDeclined";
                    break;
                case "4000000000009995":
                    testToken = "pm_card_chargeDeclined";
                    break;
                default:
                    testToken = "pm_card_visa";
                    break;
            }

            return testToken;
        }

        public string GetPublishableKey() => _publishableKey;

        public bool IsTestMode() => _isTestMode;
    }
}