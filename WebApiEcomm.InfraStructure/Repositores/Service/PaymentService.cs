using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using WebApiEcomm.Core.Entites.Basket;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Services;
using WebApiEcomm.InfraStructure.Configuration;
using WebApiEcomm.InfraStructure.Data;

namespace WebApiEcomm.InfraStructure.Repositores.Service
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly StripeMergedSettings _stripe;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            StripeMergedSettings stripe,
            AppDbContext context,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _stripe = stripe;
            _context = context;
            _logger = logger;
        }

        public async Task<CustomerBasket?> CreateOrUpdatePaymentAsync(string basketId, int deliveryMethodId)
        {
            if (string.IsNullOrWhiteSpace(basketId))
                throw new ArgumentException("Basket id is required.", nameof(basketId));

            var basket = await _unitOfWork.CustomerBasketRepository.GetCustomerBasketAsync(basketId).ConfigureAwait(false);
            if (basket is null)
                return null;

            if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
            {
                _logger.LogWarning("Stripe secret key is not configured.");
                throw new InvalidOperationException("Payment provider is not configured.");
            }

            StripeConfiguration.ApiKey = _stripe.SecretKey;

            decimal shipping = 0m;
            if (deliveryMethodId > 0)
            {
                var deliveryMethod = await _context.DeliveryMethods.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == deliveryMethodId)
                    .ConfigureAwait(false);
                shipping = deliveryMethod?.Price ?? 0m;
            }

            decimal subtotal = 0m;
            foreach (var item in basket.basketItems)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(item.Id).ConfigureAwait(false);
                if (product is null)
                {
                    _logger.LogWarning("Basket line references missing product id {ProductId}; skipping line total.", item.Id);
                    continue;
                }

                item.Price = product.NewPrice;
                subtotal += item.Price * item.Quantity;
            }

            var total = subtotal + shipping;
            var amountCents = (long)Math.Round(total * 100m, MidpointRounding.AwayFromZero);
            if (amountCents <= 0)
            {
                _logger.LogWarning("Payment amount is zero for basket {BasketId}.", basketId);
                throw new InvalidOperationException("Basket total must be greater than zero.");
            }

            var paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent;

            try
            {
                if (string.IsNullOrWhiteSpace(basket.PaymentIntentId))
                {
                    paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
                    {
                        Amount = amountCents,
                        Currency = "usd",
                        Description = "Payment for order",
                        Metadata = new Dictionary<string, string> { { "BasketId", basketId } }
                    }).ConfigureAwait(false);
                }
                else
                {
                    paymentIntent = await paymentIntentService.UpdateAsync(
                        basket.PaymentIntentId,
                        new PaymentIntentUpdateOptions { Amount = amountCents }).ConfigureAwait(false);
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment for basket {BasketId}", basketId);
                throw new InvalidOperationException("Unable to complete payment authorization. Please try again later.", ex);
            }

            basket.PaymentIntentId = paymentIntent.Id;
            basket.ClientSecret = paymentIntent.ClientSecret;

            if (basket.basketItems.Count == 0)
            {
                await _unitOfWork.CustomerBasketRepository.DeleteCustomerBasketAsync(basketId).ConfigureAwait(false);
                return null;
            }

            await _unitOfWork.CustomerBasketRepository.UpdateCustomerBasketAsync(basket).ConfigureAwait(false);
            return basket;
        }
    }
}
