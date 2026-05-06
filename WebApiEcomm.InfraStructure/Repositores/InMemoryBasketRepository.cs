using System.Text.Json;
using WebApiEcomm.Core.Entites.Basket;
using WebApiEcomm.Core.Interfaces;

namespace WebApiEcomm.InfraStructure.Repositores;

/// <summary>
/// Development fallback when Redis is not configured. Uses an in-process dictionary (JSON snapshots for isolation).
/// Registered as <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton"/> so baskets survive across HTTP requests.
/// </summary>
public sealed class InMemoryBasketRepository : ICustomerBasketRepository
{
    private readonly Dictionary<string, string> _serialized = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<bool> DeleteCustomerBasketAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult(false);

        lock (_gate)
        {
            return Task.FromResult(_serialized.Remove(id));
        }
    }

    public Task<CustomerBasket?> GetCustomerBasketAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult<CustomerBasket?>(null);

        lock (_gate)
        {
            if (!_serialized.TryGetValue(id, out var json))
                return Task.FromResult<CustomerBasket?>(null);

            try
            {
                var basket = JsonSerializer.Deserialize<CustomerBasket>(json, JsonOpts);
                return Task.FromResult(basket);
            }
            catch
            {
                return Task.FromResult<CustomerBasket?>(null);
            }
        }
    }

    public async Task<CustomerBasket?> UpdateCustomerBasketAsync(CustomerBasket customerBasket)
    {
        if (customerBasket is null || string.IsNullOrWhiteSpace(customerBasket.Id))
            return null;

        lock (_gate)
        {
            _serialized[customerBasket.Id] = JsonSerializer.Serialize(customerBasket, JsonOpts);
        }

        return await GetCustomerBasketAsync(customerBasket.Id).ConfigureAwait(false);
    }
}
