using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMVC.Models;
using WebMVC.Models.CartModels;

namespace WebMVC.Services
{
    public interface ICartService
    {
        Task<Cart> GetCart(ApplicationUser user);
        Task AddItemToCartAsync(ApplicationUser user, CartItem product);
        Task<Cart> UpdateCart(Cart Cart);
        Task<Cart> SetQuantitiesAsync(ApplicationUser user, Dictionary<string, int> quantities);
        //Order MapCartToOrder(Cart Cart);
        Task ClearCart(ApplicationUser user);

    }
}
