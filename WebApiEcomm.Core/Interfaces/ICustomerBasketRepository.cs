using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEcomm.Core.Entites.Basket;

namespace WebApiEcomm.Core.Interfaces
{
    public interface ICustomerBasketRepository
    {
        Task<CustomerBasket?> GetCustomerBasketAsync(string id);
        Task<CustomerBasket?> UpdateCustomerBasketAsync(CustomerBasket customerBasket);
        Task<bool> DeleteCustomerBasketAsync(string id);


    }
}
