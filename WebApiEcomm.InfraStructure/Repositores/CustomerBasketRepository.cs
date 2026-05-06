using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEcomm.Core.Entites.Basket;
using WebApiEcomm.Core.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace WebApiEcomm.InfraStructure.Repositores
{
    public class CustomerBasketRepository : ICustomerBasketRepository
    {
        private readonly IDatabase _database;
        public CustomerBasketRepository(IConnectionMultiplexer redis) 
        {
            _database = redis.GetDatabase();
        }

        public Task<bool> DeleteCustomerBasketAsync(string id)
        {
             return _database.KeyDeleteAsync(id);
        }

        public async Task<CustomerBasket?> GetCustomerBasketAsync(string id)
        {
            var res = await _database.StringGetAsync(id);
            if (!string.IsNullOrEmpty(res)) 
            {
                return JsonSerializer.Deserialize<CustomerBasket>(res);
            }
            return null; 
        }

        public async Task<CustomerBasket?> UpdateCustomerBasketAsync(CustomerBasket customerBasket)
        {
            var _basket = await _database.StringSetAsync(customerBasket.Id , JsonSerializer.Serialize(customerBasket) , TimeSpan.FromDays(3));
            if (customerBasket != null) 
            {
                return await GetCustomerBasketAsync(customerBasket.Id);
            }
            return null;
        }
    }
}
